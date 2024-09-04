using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//�÷��̾� ��ü
public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetUp))] //Hook?
    public int myOrder;
    public Hand hand { get; private set; }
    public Field field { get; private set; }

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

        if (isLocalPlayer)
            //�ڽ��� ī�޶� ã��
            Camera.main.GetComponent<CameraController>().Init(value);

    }

    [ClientRpc]
    public void StartTurn()
    {
        print($"{myOrder + 1}��° �÷��̾��� ����");
        //�ڽ��� ���ʸ� ����
        //��� Ŭ���̾�Ʈ���� �÷��̾� ��ü���� ����Ǿ�� �� �͵�...
        // 1) ���ʸ� �˸��� UI Pop Up
        // ī�޶� �ڽ��� ȭ���� �ָ��ϵ��� �ٲ�

        // 2) UI�� ����� �� ���� �÷��̾���� ��ο� UI�� �˾�
        if (!isLocalPlayer) return;

        //���� �÷��̾�� ������ ī�带 ��ο��� �� �ֵ���
        //��ο� UI�� ��Ÿ����.
    }

    [ClientRpc]
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
