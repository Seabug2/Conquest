using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� : ���� �ִ� ī���� ID�� ����Ʈ�� �����ϰ��ִ� Ŭ����
/// </summary>
public class Deck : NetworkBehaviour
{
    public readonly SyncList<int> list = new SyncList<int>();

    /// <summary>
    /// ���� ���� ī���� ��
    /// </summary>
    public int Count => list.Count;


    [Server]
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

    /// <returns>�� �� ���� ī�� ID�� ��ȯ�մϴ�. �� �� ���� ī���, ����Ʈ�� ������ ����Դϴ�.</returns>
    [Server]
    public int DrawCardID(bool isTopCard = true)
    {
        int drawNumber = isTopCard ? list[Count - 1] : Random.Range(0, Count);
        list.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    Deck ReturnCard(int id, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (list.Contains(id)) return this;
        if (id < 0 || id >= GameManager.TotalCard) return this;

        //placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        list.Add(id);

        if (!placeOnTop)
            Shuffle();

        return this;
    }

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

    [SerializeField, Header("���� ���� �ð��� ī�带 ���� ��ġ")]
    Transform[] draftZone;

    //���������� ����
    [ServerCallback]
    public void ServerDraftPhase()
    {
        if (Count.Equals(0))
        {
            Debug.Log("���� ī�尡 �����ϴ�!");
            //���� ����??
            return;
        }

        //���� �÷��̾� ����ŭ�� Int �迭�� �����
        int playerCount =  GameManager.AliveCount;
        int[] opened = new int[playerCount];

        //������ ī�带 4�� �̾� ��� ���� Ȯ���Ѵ�.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcDraftPhase(opened);
    }

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        for (int i = 0; i < opened.Length; i++)
        {
            Card c = GameManager.Card(opened[i]);
            c.IsOpened = true;
            c.handler.SetPosition(draftZone[i].position);
            c.handler.DoMove(i * .18f);
        }

        //"���� ���� �ð�" �޽��� ���
        //������ �ð� �ʿ�
        //���� �ð� �ʿ�
        //�޽��� ����� �����
        // => ���� �÷��̾��� ������ 0�� ��� ī�带 ������...
    }
}
