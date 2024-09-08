using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar, SerializeField, Header("���� ���� ����")]
    bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    [SyncVar]
    public int myOrder;

    #region ��, ��, �ʵ�, ī�޶� ��Ʈ�ѷ�
    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck cardManager;
    public Deck CardManager => cardManager;

    public bool IsTurnSkipped { get; internal set; }

    CameraController camCtrl;
    #endregion

    public override void OnStartClient()
    {
        GameManager.instance.AddPlayer(this);
    }

    /*
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }
    */
    public void SetUp(int order)
    {
        myOrder = order;
        field = GameManager.instance.Fields[myOrder];
        hand = GameManager.instance.Hands[myOrder];
        camCtrl = Camera.main.GetComponent<CameraController>();
        camCtrl.Init(myOrder);
    }

    [Command]
    public void CmdStartTurn()
    {
        int drawnID = cardManager.DrawCardID();

        RpcStartTurn(drawnID);
    }

    public event Action OnDrawAction;

    [ClientRpc]
    void RpcStartTurn(int drawnID)
    {
        Hand.Add(GameManager.Card(drawnID)); //��� Ŭ���̾�Ʈ�� ����Ʈ ��ü�� Hand�� �ش� ID�� ���� Villain�� �߰�
        OnDrawAction?.Invoke();

        camCtrl.SetVCam(myOrder); //���� ������ �ʵ�� �̵�

        Debug.Log($"{myOrder + 1}��° �÷��̾��� ����");

        // �ڽ��� ���ʸ� ����
        // ��� Ŭ���̾�Ʈ���� �÷��̾� ��ü���� ����Ǿ�� �� �͵�...
        // 1) ���ʸ� �˸��� UI Pop Up
        // ī�޶� �ڽ��� ȭ���� �ָ��ϵ��� �ٲ�

        // 2) UI�� ����� �� ���� �÷��̾���� ��ο� UI�� �˾�
        if (!isLocalPlayer) return;

        //���� �÷��̾�� ������ ī�带 ��ο��� �� �ֵ���
        //��ο� UI�� ��Ÿ����.

        StartCoroutine(MyTurn());

        //���ʸ� �ѱ� �� �ִ� ��ư�� Ȱ��ȭ ��
    }

    /* ���� �ؾ��� �۾�
     * �� ���� UI ��ư�� Ȱ��ȭ �� �� �ֵ��� ���� �ؾ���
     * 
     */
    public event Action OnStartTurnAction;

    [HideInInspector]
    public bool isMyTurn = false;

    IEnumerator MyTurn()
    {
        camCtrl.SetVCam(myOrder);
        //ȭ���� �ű� �� ����
        isMyTurn = true;

        OnStartTurnAction?.Invoke();

        //ȭ���� �ű� �� ����
        while (isMyTurn)
        {
            //�ڽ��� �п��� �� �� �ִ� ī�尡 ���� ��� �ڽ��� ���ʸ� ������
            //if(�ڽ��� �п���...)
            isMyTurn = false;

            if (!isMyTurn)
            {
                ///////////////���⼭ 
                yield break;
            }

            //���ʸ� �����ϰ� ������ 
            yield return null;
        }

        camCtrl.SetVCam(myOrder);
        //ȭ���� �ű� �� ����
    }

    [Command]
    void CmdEndTurn()
    {
        if (hand.IsLimitOver())
        {
            isGameOver = true;
            GameManager.instance.GameOver(myOrder);
            return;
        }
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {

        print($"{myOrder + 1}��° �÷��̾ ���ʸ� ��ħ");

        // �� ���� UIȿ��
        // ��� �÷��̾� ȭ���̵�...

        //if (isLocalPlayer)
        //    GameManager.instance.NextTurn(myOrder);
    }


    public void SelectCard()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.GetMouseButtonDown(1));
    }

    IEnumerator SelectCard_co()
    {
        while (true)
        {

            yield return null;
        }
    }
}
 