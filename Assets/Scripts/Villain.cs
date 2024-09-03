using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//�÷��̾� ��ü
public class Villain : NetworkBehaviour
{
    [SyncVar] //Hook?
    public int myOrder = 0;

    Hand hand;
    Field field;

    private void Awake()
    {
        hand = GetComponent<Hand>();
    }

    private void Start()
    {
        if (isServer)
            //���ӸŴ����� ã�Ƽ�
            FindObjectOfType<GameManager>().AddPlayer(this);
    }

    [ClientRpc]
    public void SetUp(int i)
    {
        myOrder = i;
        if (!isLocalPlayer) return;

        //�ڽ��� ī�޶� ã��
        Camera.main.GetComponent<CameraController>().Init(i);

        //�ڽ��� �ʵ� ã��
        Field[] fields = FindObjectsOfType<Field>();
        foreach (Field f in fields)
        {
            if(f.seatNum == myOrder)
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
    }

    [ClientRpc]
    public void StartTurn()
    {
        //�ڽ��� ���ʸ� ����
        //��� Ŭ���̾�Ʈ���� �÷��̾� ��ü���� ����Ǿ�� �� �͵�...
        // 1) ���ʸ� �˸��� UI Pop Up

        // 2) UI�� ����� �� ���� �÷��̾���� ��ο� UI�� �˾�
        if (!isLocalPlayer) return;

        //���� �÷��̾�� ������ ī�带 ��ο��� �� �ֵ���
        //��ο� UI�� ��Ÿ����.
    }
}
