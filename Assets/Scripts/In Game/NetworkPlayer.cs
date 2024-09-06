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

    [SyncVar(hook = nameof(SetUp))] //���� �Ҵ��Ϸ��� ���� ������ ���� ������ ȣ���� �ȵ�
    public int myOrder = -1;

    #region ��, ��, �ʵ�, ī�޶� ��Ʈ�ѷ�
    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck deck;
    public Deck Deck => deck;

    CameraController camCtrl;
    #endregion

    private void Start()
    {
        //��� Ŭ���̾�Ʈ���� ������ �÷��̾� ��ü�� ������ GameManage�� Player List�� �߰���
        GameManager.instance.AddPlayer(this);
    }

    public void SetUp(int _, int value)
    {
        myOrder = value;
        isGameOver = false;

        //�ڽ��� �ʵ� ã��
        Field[] fields = FindObjectsOfType<Field>();
        foreach (Field f in fields)
        {
            if (f.seatNum == myOrder)
            {
                field = f;
                break;
            }
        }

        //�ڽ��� �ڵ� ã��
        Hand[] hands = FindObjectsOfType<Hand>();
        foreach (Hand h in hands)
        {
            if (h.seatNum == myOrder)
            {
                hand = h;
                break;
            }
        }

        deck = FindObjectOfType<Deck>();

        camCtrl = Camera.main.GetComponent<CameraController>();
        camCtrl.Init(value);
    }

    [Command]
    public void CmdStartTurn()
    {
        //���ʸ� �����ϸ� ���� ���� ������ ī�带 ��ο� �մϴ�.

        if (deck.Count == 0)
        {
            Debug.Log("���� �� �嵵 �����ϴ�!");
            return;
        }

        //���� ������ �ϰ� �ִ� Ŭ���̾�Ʈ�� ����Ʈ ��ü���� ȣ��
        int drawnID = UnityEngine.Random.Range(0, deck.Count);
        deck.CmdDraw(drawnID); //������ �������� ������ ������ List���� ���ŵ�

        RpcStartTurn(drawnID);
    }

    public event Action OnDrawAction;

    [ClientRpc]
    void RpcStartTurn(int drawnID)
    {
        Hand.AddHand(Deck.Cards[drawnID]); //��� Ŭ���̾�Ʈ�� ����Ʈ ��ü�� Hand�� �ش� ID�� ���� Villain�� �߰�
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
        if (hand.IsGameOver())
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

        if (isLocalPlayer)
            GameManager.instance.NextTurn(myOrder);
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
