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
    [Server]
    public void ServerDraftPhase()
    {
        //���� ������ �÷��̾� �� ��ŭ ī�带 �����Ѵ�.
        int playerCount = GameManager.AliveCount;
        int[] drawnCard = new int[playerCount];
        draftCard.Clear();

        if (Count == 0)
        {
            Debug.LogError("���� ī�尡 �����ϴ�!");
        }
        else
        {
            for (int i = 0; i < playerCount; i++)
            {
                int id = DrawCardID();
                draftCard.Add(id);
                drawnCard[i] = id;
            }
        }

        GameManager.instance.SetFirstPlayer();

        float t = 10;
        Commander commander = new();
        commander
            .WaitUntil(() => GameManager.instance.IsAllReceived)
            .OnUpdate(() =>
            {
                t -= Time.deltaTime;
                if (t > 0) return;
                GameManager.instance.CheckDisconnectedPlayers();
                commander.Cancel();
            })
            .OnCompleteAll(() =>
            {
                RpcDraftPhase(drawnCard, GameManager.CurrentOrder);
                GameManager.instance.ResetAcknowledgements();
            })
            .Play();
    }

    readonly SyncList<int> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard, int firstPlayerOrder)
    {
        //�÷��̾���� ȭ���� ���ͷ� �̵���Ű�� ī�޶� �̵��� �Ұ����ϰ� �Ѵ�
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);

        Commander commander = new();
        commander
            .Add(() => UIManager.Message.PopUp("���� ���� �ð�!", 3f), 1f)
            .Add(() =>
            {
                int count = _draftCard.Length;
                float intervalAngle = 360f / count;
                float angle = Random.Range(0f, 90f);

                for (int i = 0; i < count; i++)
                {
                    Card c = GameManager.Card(_draftCard[i]);
                    //�Ź� �������� ���� �̱��濡 ĳ���� ���¸� ���
                    c.iCardState = GameManager.instance.noneState;
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
            .Add(() => ClientSelectDraftCard(firstPlayerOrder))
            .Play();
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        ClientSelectDraftCard(_order);
    }

    [Client]
    void ClientSelectDraftCard(int _order)
    {
        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            UIManager.Message.PopUp("�з� ������ ī�带 �� �� ������", 3f);

            foreach (int id in draftCard)
            {
                Card card = GameManager.Card(id);

                card.iCardState = new SelectionState(card
                    , () => UIManager.Confirm.PopUp(() =>
                    {
                        //���� ��� ��� ī�带 ���� �Ұ����� ���·� �ٲ۴�. 
                        foreach (int id in draftCard)
                        {
                            Card c = GameManager.Card(id);
                            c.iCardState = GameManager.instance.noneState;
                        }
                        CmdSelectDraftCard(card.id);
                    }, "�� ī�带 �з� �������ϴ�?"
                    , card.front));
            }
        }
        else
        {
            UIManager.Message.ForcePopUp($"{_order + 1}��° �÷��̾\nī�带 ���� ���Դϴ�.", 3f);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard(int id)
    {
        //���������� ó��
        draftCard.Remove(id);

        GameManager.GetPlayer(GameManager.CurrentOrder).Hand.RpcAdd(id);

        if (draftCard.Count > 1)
        {
            GameManager.CurrentOrder = GameManager.instance.NextOrder(GameManager.CurrentOrder);
            RpcSelectDraftCard(GameManager.instance.NextOrder(GameManager.CurrentOrder));
        }
        else
        {
            int lastID = draftCard[0];
            GameManager.GetPlayer(GameManager.CurrentOrder).Hand.RpcAdd(lastID);

            GameManager.instance.SetFirstPlayer();

            float t = 10f;
            Commander commander = new();
            commander
                .WaitUntil(() => GameManager.instance.IsAllReceived)
                .OnUpdate(() =>
                {
                    t -= Time.deltaTime;
                    if (t > 0) return;
                    GameManager.instance.CheckDisconnectedPlayers();
                    commander.Cancel();
                })
                .OnComplete(() =>
                {
                    GameManager.instance.ResetAcknowledgements();
                    RpcEndSelectionDraftCard();
                })
                .Play();
        }
    }

    [ClientRpc]
    void RpcEndSelectionDraftCard()
    {
        Commander commander = new Commander()
            .Add(() => UIManager.Message.ForcePopUp("ī�� ������ ���ƽ��ϴ�!", 3f))
            .WaitSeconds(3.3f)
            .Add(GameManager.GetPlayer(GameManager.CurrentOrder).ClientStartTurn);
        commander.Play();
    }

    #endregion
}
