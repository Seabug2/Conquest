using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

//�÷��̾� ��ü
public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetUp))] //Hook?
    public int myOrder = -1;

    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck deck;
    public Deck Deck => deck;

    private void Start()
    {
        //Host�� ���� �÷��̾� ������Ʈ�� �߰��� ������ GameManager�� Player Dictionary�� ������Ʈ�� �߰��Ѵ�.
        if (isServer)
            GameManager.instance.AddPlayer(this);
    }

    public void SetUp(int _, int value)
    {
        //????????????????????????????????????????????????????????????????????????ȭ�鿡�� Waiting UI ��Ȱ��ȭ / ����

        myOrder = value;

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

        if (isLocalPlayer)
            //�ڽ��� ī�޶� ã��
            Camera.main.GetComponent<CameraController>().Init(value);

        //�ڽ��� ù ��° �÷��̾��� ���� ���ʸ� �����Ѵ�.
        if (myOrder == 0 && isLocalPlayer)
        {
            CmdStartTurn();
        }
    }

    [Command]
    public void CmdStartTurn()
    {
        if (deck.Count == 0)
        {
            Debug.Log("���� �� �嵵 �����ϴ�!");
            return;
        }

        int drawnID = Random.Range(0, deck.Count);
        deck.CmdDraw(drawnID); //������ �������� ������ ������ List���� ���ŵ�

        RpcStartTurn(drawnID);
    }

    [ClientRpc]
    void RpcStartTurn(int drawnID)
    {
        Hand.AddHand(Deck.Villains[drawnID]);

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
