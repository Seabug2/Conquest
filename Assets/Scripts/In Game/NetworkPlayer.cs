using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

//�÷��̾� ��ü
public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar, SerializeField, Header("���� ���� ����")]
    bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    [SyncVar(hook = nameof(SetUp))] //���� �Ҵ��Ϸ��� ���� ������ ���� ������ ȣ���� �ȵ�
    public int myOrder = -1;

    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck deck;
    public Deck Deck => deck;

    CameraController camCtrl;

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

        //���ʸ� �ѱ� �� �ִ� ��ư�� Ȱ��ȭ ��
    }

    [Command]
    public void EndTurn()
    {
        if (hand.IsGameOver)
        {
            isGameOver = false;
            GameManager.instance.GameOver(myOrder);
            return;
        }

        print($"{myOrder + 1}��° �÷��̾ ���ʸ� ��ħ");

        // �� ���� UIȿ��
        // ��� �÷��̾� ȭ���̵�...

        if (isLocalPlayer)
            GameManager.instance.NextTurn(myOrder);
    }
}
