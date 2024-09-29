using System;
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
            int rand = UnityEngine.Random.Range(i, Count); // i���� Count-1������ �ε����� �������� ����
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
        int drawNumber = isTopCard ? list[Count - 1] : UnityEngine.Random.Range(0, Count);
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
    readonly SyncList<int> draftCard = new();

    [Server]
    public void ServerDraftPhase()
    {
        if (isServerOnly)
            GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        //10�� ���...
        float t = 10;

        Commander commander = new Commander()
            .Add(GameManager.instance.SetNewRound)
            .WaitWhile(() => true)
            .CancelTrigger(() => GameManager.instance.IsAllReceived() || t <= 0)
            .OnCanceled(() =>
            {
                if (!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();

                GameManager.instance.ResetAcknowledgements();

                if (Count == 0)
                {
                    Debug.LogError("���� ī�尡 �����ϴ�!");
                    //TODO
                    return;
                }

                //���� ������ �÷��̾� �� ��ŭ ī�带 �����Ѵ�.
                int playerCount = Mathf.Min(GameManager.AliveCount, Count);

                int[] drawnCard = new int[playerCount];

                draftCard.Clear();

                for (int i = 0; i < playerCount; i++)
                {
                    int id = DrawCardID();
                    draftCard.Add(id);
                    drawnCard[i] = id;
                }
                RpcDraftPhase(drawnCard);
            })
            .OnUpdate(() => t -= Time.deltaTime)
            .Play();
    }

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard)
    {
        GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;

        new Commander()
            .Add(() =>
            {
                //�÷��̾���� ȭ���� ���ͷ� �̵���Ű�� ī�޶� �̵��� �Ұ����ϰ� �Ѵ�
                CameraController.instance.FocusOnCenter();
                CameraController.instance.MoveLock(true);
                UIManager.GetUI<LineMessage>().PopUp("���� ���� �ð�!", 3f);
            }, 1f)
            .Add(() =>
            {
                int count = _draftCard.Length;
                float intervalAngle = 360f / count;
                float angle = UnityEngine.Random.Range(0f, 90f);

                for (int i = 0; i < count; i++)
                {
                    Card c = GameManager.Card(_draftCard[i]);
                    c.iCardState = GameManager.instance.noneState;
                    c.IsOpened = true;

                    angle += intervalAngle;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * 1.5f;
                    c.SetTargetPosition(transform.position + position);
                    c.SetTargetQuaternion(Quaternion.Euler(0, 0, UnityEngine.Random.Range(-6f, 6f))); //10�� �̻����� ȸ���ϸ� ����ϰ� ����

                    c.DoMove(i * .18f);
                }
            })
            .WaitWhile(isPlaying)
            .Add(() => ClientSelectDraftCard(GameManager.FirstOrder))
            .Play();
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        ClientSelectDraftCard(_order);
    }

    void EndSelection(int _order)
    {
        if (UIManager.GetUI<Confirm>().IsActive)
        {
            UIManager.GetUI<Confirm>().Close();
        }

        foreach (int id in draftCard)
        {
            Card c = GameManager.Card(id);
            c.iCardState = GameManager.instance.noneState;
        }

        int rand = UnityEngine.Random.Range(0, draftCard.Count);
        CmdSelectDraftCard(_order, draftCard[rand]);
    }

    [Client]
    void ClientSelectDraftCard(int _order)
    {
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);
        UIManager.GetUI<Timer>().@Reset();

        Commander commander = new();

        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            commander
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print("ī�� ����");
                    UIManager.GetUI<LineMessage>().PopUp("�з� ������ ī�带 �� �� ������", 3f);
                }
                , 3f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);

                    //���� ������ ī���� ���¸� �ٲ�
                    foreach (int id in draftCard)
                    {
                        Card card = GameManager.Card(id);

                        card.iCardState = new SelectionState(card
                            , () => UIManager.GetUI<Confirm>().PopUp(() =>
                            {
                                UIManager.GetUI<Timer>().Stop();

                                //Ȯ�� ��ư�� ������...
                                //���� ��� ��� ī�带 ���� �Ұ����� ���·� �ٲ۴�. 
                                foreach (int id in draftCard)
                                {
                                    Card c = GameManager.Card(id);
                                    c.iCardState = GameManager.instance.noneState;
                                }

                                CmdSelectDraftCard(GameManager.LocalPlayer.Order, card.id);
                            }, "�� ī�带 �з� �������ϴ�?"
                            , card.front));
                    }

                    //30��
                    UIManager.GetUI<Timer>().Play(30f, () => EndSelection(_order));
                })
                .Play();
        }
        else
        {
            commander
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print("���� ���� �ð�");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{_order + 1}��° �÷��̾\nī�带 ���� ���Դϴ�.", 3f);
                }, 3f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().Play(30f);
                })
                .Play();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard(int order, int id)
    {
        draftCard.Remove(id);
        GameManager.GetPlayer(order).Hand.RpcAdd(id);

        order = GameManager.instance.NextOrder(order);

        if (draftCard.Count > 1)
        {
            GameManager.instance.SetCurrentOrder(order);
            RpcSelectDraftCard(order);
            return;
        }

        //���� ī�尡 �� ���� ���
        //���� �� ���� ���� �÷��̾��� �п� �߰�
        int lastID = draftCard[0];
        draftCard.Clear();
        GameManager.GetPlayer(order).Hand.RpcAdd(lastID);

        float t = 10f;
        Commander commander = new();
        commander
            .Add(GameManager.instance.SetNewRound)
            .WaitWhile(() => true)
            .CancelTrigger(() => GameManager.instance.IsAllReceived() || t <= 0)
            .OnCanceled(() =>
            {
                if (!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();

                GameManager.instance.ResetAcknowledgements();

                if (isServerOnly)
                    GameManager.instance.CurrentPhase = GamePhase.PlayerPhase;

                RpcEndSelectionDraftCard(GameManager.FirstOrder);
            })
            .OnUpdate(() => t -= Time.deltaTime)
            .Play();
    }

    [ClientRpc]
    void RpcEndSelectionDraftCard(int _firstOrder)
    {
        GameManager.instance.CurrentPhase = GamePhase.PlayerPhase;
        UIManager.GetUI<Timer>().Stop();

        new Commander()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("ī�� ������ ���ƽ��ϴ�!", 3f), 3f)
            .Add(GameManager.GetPlayer(_firstOrder).ClientStartTurn)
            .Play();
    }

    #endregion
}
