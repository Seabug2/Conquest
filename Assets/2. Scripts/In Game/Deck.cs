using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� : ���� �ִ� ī���� ID�� ����Ʈ�� �����ϰ��ִ� Ŭ����
/// </summary>
public class Deck : NetworkBehaviour
{
    public readonly SyncList<int> list = new();

    #region �� �ʱ�ȭ
    private void Start()
    {
        GameManager.deck = this;
    }

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
    #endregion


    #region �� ����
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
    #endregion






    #region ���翵�Խð�
    [ServerCallback]
    public void ServerDraftPhase()
    {
        //���� �÷��̾� ����ŭ�� Int �迭�� �����
        int playerCount = GameManager.AliveCount;
        int[] opened = new int[playerCount];

        if (Count == 0)
        {
            Debug.LogError("���� ī�尡 �����ϴ�!");
        }
        else
        {
            //������ ī�带 4�� �̾� ��� ���� Ȯ���Ѵ�.
            for (int i = 0; i < playerCount; i++)
            {
                int id = DrawCardID();
                opened[i] = id;
            }
        }

        RpcDraftPhase(opened);
    }

    readonly List<Card> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        //����Ʈ�� ���� �� ��, ���Ҵ��� �ƴ϶� Clear()
        draftCard.Clear();

        Commander commander = new();
        commander
            .Add(() => UIManager.Message.PopUp("���� ���� �ð�!", 3f), 1f)
            .Add(() =>
            {
                int count = opened.Length;
                float intervalAngle = 360f / count;
                float angle = Random.Range(0, 90);

                for (int i = 0; i < opened.Length; i++)
                {
                    Card c = GameManager.Card(opened[i]);
                    draftCard.Add(c);
                    c.iCardState = new NoneState();
                    c.IsOpened = true;

                    angle += intervalAngle;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * 1.5f;
                    c.SetTargetPosition(transform.position + position);
                    c.SetTargetQuaternion(Quaternion.Euler(0, 0, Random.Range(-8f, 8f))); //10�� �̻����� ȸ���ϸ� ����ϰ� ����

                    c.DoMove(i * .18f);
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(StartCardSelection)
            .Play();
    }

    [ServerCallback]
    void StartCardSelection()
    {
        int firstOrder = GameManager.instance.FirstOrder();
        RpcSelectDraftCard(firstOrder);
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard()
    {
        if (GameManager.RoundFinished)
        {
            //Todo �÷��̾� ���� �� ����
            //GameManager.instance.
        }
        else
        {
            RpcSelectDraftCard(GameManager.instance.NextOrder(GameManager.instance.CurrentOrder));
        }
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        GameManager.instance.CurrentOrder = _order;

        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            UIManager.Message.PopUp("�з� ������ ī�带 �� �� ������", 3f);
            foreach (Card card in draftCard)
            {
                card.iCardState = new SelectionState(card
                    , () => UIManager.Confirm.PopUp(() =>
                    {
                        GameManager.instance.LocalPlayer.Hand.CmdAdd(card.id);
                        draftCard.Remove(card);

                        foreach (Card c in draftCard)
                        {
                            c.iCardState = new NoneState();
                        }

                        CmdSelectDraftCard();
                    }, "�� ī�带 �з� �������ϴ�?"
                    , card.front));
            }
        }
        else
        {
            UIManager.Message.ForcePopUp($"{GameManager.instance.CurrentOrder + 1}��° �÷��̾ ī�带 ���� ���Դϴ�.", 5f);
        }
    }
    #endregion
}
