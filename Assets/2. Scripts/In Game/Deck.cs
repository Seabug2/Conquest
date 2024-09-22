using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� : ���� �ִ� ī���� ID�� ����Ʈ�� �����ϰ��ִ� Ŭ����
/// </summary>
public class Deck : NetworkBehaviour
{
    private void Start()
    {
        GameManager.deck = this;
    }

    public readonly SyncList<int> list = new();

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

    [Server]
    /// <returns>�� �� ���� ī�� ID�� ��ȯ�մϴ�. �� �� ���� ī���, ����Ʈ�� ������ ����Դϴ�.</returns>
    public int DrawCardID(bool isTopCard = true)
    {
        if (Count.Equals(0))
        {
            Debug.LogError("���� ī�尡 �����ϴ�!");
            return -1;
        }
        int drawNumber = isTopCard ? list[Count - 1] : Random.Range(0, Count);
        list.Remove(drawNumber);
        return drawNumber;
    }

    /// <summary>
    /// _placeOnTop�� true��� ī�带 �� �� ���� �д�.
    /// _placeOnTop�� false��� ���� ī�带 �ְ� ���´�.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdReturnCard(int _id, bool _placeOnTop)
    {
        //�̹� �� �ȿ� �ִ� ī���� �ٽ� ���� ���� �� ����.
        if (list.Contains(_id))
        {
            Debug.LogError("�̹� ���� �ִ� ī�带 �߰��Ϸ��� �߽��ϴ�. �߸��� ��Ȳ�Դϴ�.");
            return;
        }
        if (_id < 0 || _id >= GameManager.TotalCard)
        {
            Debug.LogError("�߸��� ī�� ID�Դϴ�. �߸��� ��Ȳ�Դϴ�.");
            return;
        }

        list.Add(_id);

        //placeOnTop�� true�� ���� �� ����, false�� ���� ��ġ�� ����
        if (!_placeOnTop) Shuffle();

        RpcReturnCard(_id);
    }

    [ClientRpc]
    void RpcReturnCard(int _id)
    {
        Card card = GameManager.Card(_id);
        card.iCardState = new InDeckState(card);
        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove();
    }


    //���������� ����
    [ServerCallback]
    public void ServerDraftPhase()
    {
        if (Count.Equals(0))
        {
            Debug.LogError("���� ī�尡 �����ϴ�!");
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

    [SerializeField, Header("���� ������ �ð��� ī�带 ���� ��ġ"), Space(10)]
    Transform[] draftZone;
    readonly List<Card> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        //����Ʈ�� ���� �� ��, ���Ҵ��� �ƴ϶� Clear()
        draftCard.Clear();

        Commander commander = new();
        commander
            .Add(() => UIMaster.Message.PopUp("���� ���� �ð�!", 3f), 1f)
            .Add_While(() =>
            {
                for (int i = 0; i < opened.Length; i++)
                {
                    Card c = GameManager.Card(opened[i]);
                    draftCard.Add(c);
                    c.iCardState = new NoneState();
                    c.IsOpened = true;
                    c.SetTargetPosition(draftZone[i].position);
                    c.SetTargetQuaternion(Quaternion.identity);

                    c.DoMove(i * .18f);
                }
            }, UIMaster.Message.IsPlaying)
            .Add_While(() =>
            {
                //if (GameManager.LocalPlayer.order == 0)
                //{
                //    UIMaster.Message.PopUp("�з� ������ ī�带 �� �� ������", 3f);
                //    foreach (Card card in draftCard)
                //    {
                //        card.iCardState = new SelectionState(card, () => UIMaster.Confirm.PopUp(() =>
                //        {
                //            draftCard.Remove(card);

                //            GameManager.LocalPlayer.hand.Add(card.id);

                //            foreach (Card card in draftCard)
                //            {
                //                card.iCardState = new NoneState();
                //            }

                //        }, "�� ī�带 �з� �������ϴ�?", card.front));
                //    }
                //}
                //else
                //{
                //    UIMaster.Message.PopUp($"{GameManager.instance.CurrentOrder + 1}��° �÷��̾ ī�带 ���ϴ�.", 3f);
                //}
            }, UIMaster.Message.IsPlaying)
            .Play();
    }


}
