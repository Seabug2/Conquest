using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardManager : NetworkBehaviour
{
    [SerializeField] Card[] cards;
    public Card GetCard(int ID) => cards[ID];

    public readonly SyncList<int> deck = new SyncList<int>();
    /// <summary>
    /// ���� ���� ī���� ��
    /// </summary>
    public int Count => deck.Count;

    [ServerCallback]
    void Shuffle()
    {
        for (int i = 0; i < Count - 1; i++)
        {
            int rand = Random.Range(i, Count); // i���� Count-1������ �ε����� �������� ����
            int tmp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = tmp;
        }
    }

    /// <summary>
    /// ������ ī�带 �������� ������ ���...
    /// </summary>
    [Server]
    public int RandomPickUpID()
    {
        int drawNumber = deck[Random.Range(0, Count)];
        deck.Remove(drawNumber);
        return drawNumber;
    }

    /// <returns>�� �� ���� ī�� ID�� ��ȯ�մϴ�. �� �� ���� ī���, ����Ʈ�� ù ��° ����Դϴ�.</returns>
    [Server]
    public int DrawCardID()
    {
        int drawNumber = deck[0];
        deck.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    void ReturnID(int id, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (deck.Contains(id)) return;
        
        // placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        if (Count == 0)
        {
            deck.Add(id); // ���� ����ִٸ� �׳� �߰�
        }
        else
        {
            deck.Insert(placeOnTop ? 0 : Random.Range(0, Count), id);
        }
    }

    [Server]
    void ReturnID(Card card, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (deck.Contains(card.ID)) return;
        
        // placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        if (Count == 0)
        {
            deck.Add(card.ID); // ���� ����ִٸ� �׳� �߰�
        }
        else
        {
            deck.Insert(placeOnTop ? 0 : Random.Range(0, Count), card.ID);
        }
    }


    [SerializeField]
    Transform[] draftZone;

    private void Start()
    {
        if (isServer)
        {
            int length = cards.Length;
            for (int i = 0; i < length; i++)
            {
                deck.Add(i);
            }
            Shuffle();
        }
    }

    /// <param name="drawnId">������ ī���� ID</param>
    [Server]
    public void CmdRemove(int drawnId)
    {
        deck.Remove(drawnId);
    }

    /// <param name="drawnId">���� �߰��� ī���� ID</param>
    [Server]
    public void CmdAdd(int drawnId)
    {
        deck.Add(drawnId);
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

            Card c = GetCard(opened[i]);
            c.handler.SetFace(true);

            float x = draftZone[i].position.x;
            float y = draftZone[i].position.y;
            float z = draftZone[i].position.z;

            c.handler.SetPosition(x, y, z, (i + 1) * .18f);
        }
    }
}
