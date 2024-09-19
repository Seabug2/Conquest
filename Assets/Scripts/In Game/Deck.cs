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

    public override void OnStartServer()
    {
        int length = GameManager.TotalCard;
        for (int i = 0; i < length; i++)
        {
            list.Add(i);
        }
        Shuffle();
    }

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
    int DrawCardID(bool isTopCard = true)
    {
        int drawNumber = isTopCard ? list[Count - 1] : Random.Range(0, Count);
        list.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    public void ServerDrawCard(int _playerNumber, bool _isTopCard = true)
    {
        int cardID = DrawCardID(_isTopCard);
        RpcDrawCard(_playerNumber, cardID);
    }

    [ClientRpc]
    void RpcDrawCard(int _playerNumber, int _drawCardID)
    {
        GameManager.Player(_playerNumber).hand.Add(_drawCardID);
    }

    [Server]
    Deck ReturnCard(int id, bool placeOnTop = false)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (list.Contains(id))
        {
            Debug.Log("�̹� ���� �ִ� ī�带 �߰��Ϸ��� �߽��ϴ�. �߸��� ��Ȳ�Դϴ�.");
            return this;
        }
        if (id < 0 || id >= GameManager.TotalCard)
        {
            Debug.Log("�߸��� ī�� ID�Դϴ�. �߸��� ��Ȳ�Դϴ�.");
            return this;
        }

        //placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        list.Add(id);

        if (!placeOnTop)
            Shuffle();

        return this;
    }



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
        int playerCount = GameManager.AliveCount;
        int[] opened = new int[playerCount];

        //������ ī�带 4�� �̾� ��� ���� Ȯ���Ѵ�.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcDraftPhase(opened);
    }

    [SerializeField, Header("���� ���� �ð��� ī�带 ���� ��ġ"), Space(10)]
    Transform[] draftZone;

    List<Card> draftCard = new List<Card>();

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        //����Ʈ�� ���� �� ��, ���Ҵ��� �ƴ϶� Clear()
        draftCard.Clear();

        for (int i = 0; i < opened.Length; i++)
        {
            Card c = GameManager.Card(opened[i]);
            draftCard.Add(c);
            c.iCardState = new None();
            c.IsOpened = true;
            c.SetTargetPosition(draftZone[i].position);
            c.SetTargetQuaternion(Quaternion.identity);

            c.DoMove(i * .18f);
        }

        Commander commander = new Commander();
        commander
            .Add_While(() => UIMaster.Message.PopUp("���� ���� �ð�!", 3f), UIMaster.Message.IsPlaying)
            .WaitSeconds(0.3f)
            .Add(() =>
            {
                //GameManager.Player(0).
                //ù ��° �÷��̾� ���� ī�� ������ ��ȸ�� �־�����.
            })
            .Play();

        //"���� ���� �ð�" �޽��� ���
        //������ �ð� �ʿ�
        //���� �ð� �ʿ�
        //�޽��� ����� �����
        // => ���� �÷��̾��� ������ 0�� ��� ī�带 ������...
    }
}
