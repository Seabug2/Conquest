using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Deck : NetworkBehaviour
{
    public readonly SyncList<int> list = new SyncList<int>();
    /// <summary>
    /// ���� ���� ī���� ��
    /// </summary>
    public int Count => list.Count;

    [ServerCallback]
    void Shuffle()
    {
        for (int i = 0; i < Count - 1; i++)
        {
            int rand = Random.Range(i, Count); // i���� Count-1������ �ε����� �������� ����
            int tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }

    /// <summary>
    /// ������ ī�带 �������� ������ ���...
    /// </summary>
    [Server]
    public int RandomPickUpID()
    {
        int drawNumber = list[Random.Range(0, Count)];
        list.Remove(drawNumber);
        return drawNumber;
    }

    /// <returns>�� �� ���� ī�� ID�� ��ȯ�մϴ�. �� �� ���� ī���, ����Ʈ�� ù ��° ����Դϴ�.</returns>
    [Server]
    public int DrawCardID()
    {
        int drawNumber = list[0];
        list.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    void ReturnID(int id, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (list.Contains(id)) return;

        // placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        if (Count == 0)
        {
            list.Add(id); // ���� ����ִٸ� �׳� �߰�
        }
        else
        {
            list.Insert(placeOnTop ? 0 : Random.Range(0, Count), id);
        }
    }

    [Server]
    void ReturnID(Card card, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (list.Contains(card.ID)) return;

        // placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        if (Count == 0)
        {
            list.Add(card.ID); // ���� ����ִٸ� �׳� �߰�
        }
        else
        {
            list.Insert(placeOnTop ? 0 : Random.Range(0, Count), card.ID);
        }
    }


    [SerializeField]
    Transform[] draftZone;

    private void Start()
    {
        if (isServer)
        {
            int length = GameManager.TotalCard;
            for (int i = 0; i < length; i++)
            {
                list.Add(i);
            }
            Shuffle();
        }
    }

    /// <param name="drawnId">������ ī���� ID</param>
    [Server]
    public void CmdRemove(int drawnId)
    {
        list.Remove(drawnId);
    }

    /// <param name="drawnId">���� �߰��� ī���� ID</param>
    [Server]
    public void CmdAdd(int drawnId)
    {
        list.Add(drawnId);
    }

    //�ν����͸� ���� ���ӸŴ����� �̺�Ʈ�� �����Ͽ� ���
    [ServerCallback]
    public void DraftPhase()
    {
        //���� �÷��̾� ����ŭ�� Int �迭�� �����
        int playerCount = GameManager.instance.AliveCount;
        int[] opened = new int[playerCount];

        //������ ī�带 4�� �̾� ��� ���� Ȯ���Ѵ�.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcOpenDraftCard(opened);
    }

    /// <param name="opened">Card IDs</param>
    [ClientRpc]
    void RpcOpenDraftCard(int[] opened)
    {
        for (int i = 0; i < opened.Length; i++)
        {

            Card c = GameManager.Card(opened[i]);
            c.IsOpened = true;

            float x = draftZone[i].position.x;
            float y = draftZone[i].position.y;
            float z = draftZone[i].position.z;

            c.handler.isSelectable = false;
            c.handler.SetPosition(x, y, z);
            c.handler.DoMove(i * .18f);
        }
    }
}
