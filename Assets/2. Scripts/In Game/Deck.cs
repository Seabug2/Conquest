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
    readonly SyncList<int> draftCard = new();

    [Server]
    public void ServerDraftPhase()
    {
        //10�� ���...
        float t = 10;

        Commander commander = new();
        commander
            .Add(GameManager.instance.SetNewRound)
            .WaitUntil(GameManager.instance.IsAllReceived)
            .OnUpdate(() =>
            {
                t -= Time.deltaTime;
                if (t > 0) return;
                //10�ʰ� ���� ������ ������ ���� ���ƿ��� ���� ���...
                GameManager.instance.CheckDisconnectedPlayers();
                commander.Cancel();
            })
            .OnCompleteAll(() =>
            {
                GameManager.instance.ResetAcknowledgements();

                //���� ������ �÷��̾� �� ��ŭ ī�带 �����Ѵ�.
                int playerCount = GameManager.AliveCount;
                int[] drawnCard = new int[playerCount];

                draftCard.Clear();

                for (int i = 0; i < playerCount; i++)
                {
                    if (Count == 0)
                    {
                        Debug.LogError("���� ī�尡 �����ϴ�!");
                        break;
                    }
                    int id = DrawCardID();
                    draftCard.Add(id);
                    drawnCard[i] = id;
                }
                RpcDraftPhase(drawnCard);
            })
            .Play();
    }

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard)
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
                    c.iCardState = GameManager.instance.noneState;
                    c.IsOpened = true;

                    angle += intervalAngle;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * 1.5f;
                    c.SetTargetPosition(transform.position + position);
                    c.SetTargetQuaternion(Quaternion.Euler(0, 0, Random.Range(-6f, 6f))); //10�� �̻����� ȸ���ϸ� ����ϰ� ����

                    c.DoMove(i * .18f);
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(() => ClientSelectDraftCard(GameManager.FirstOrder))
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
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);

        //ī�带 �����ϴ� �ð��� 30�� ����
        float t = 30f;

        Commander commander = new();
        commander
            .Add(() =>
            {
                if (GameManager.GetPlayer(_order).isLocalPlayer)
                {
                    UIManager.Message.PopUp("�з� ������ ī�带 �� �� ������", 3f);
                }
                else
                {
                    UIManager.Message.ForcePopUp($"{_order + 1}��° �÷��̾\nī�带 ���� ���Դϴ�.", 3f);
                }
            })
            .WaitSeconds(3f)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);

                if (!GameManager.GetPlayer(_order).isLocalPlayer)
                {
                    commander.Cancel();
                    return;
                }

                t += 3f;
                foreach (int id in draftCard)
                {
                    Card card = GameManager.Card(id);

                    card.iCardState = new SelectionState(card
                        , () => UIManager.Confirm.PopUp(() =>
                        {
                            //Ȯ�� ��ư�� ������...
                            //���� ��� ��� ī�带 ���� �Ұ����� ���·� �ٲ۴�. 
                            foreach (int id in draftCard)
                            {
                                Card c = GameManager.Card(id);
                                c.iCardState = GameManager.instance.noneState;

                                commander.Cancel();
                            }
                            CmdSelectDraftCard(GameManager.LocalPlayer.order, card.id);
                        }, "�� ī�带 �з� �������ϴ�?"
                        , card.front));
                }
            })
            .WaitWhile(() => true)
            .OnUpdate(() =>
            {
                t -= Time.deltaTime;
                if (t <= 0)
                {
                    if (UIManager.Confirm.IsActive)
                    {
                        UIManager.Confirm.Close();
                    }

                    int rand = Random.Range(0, draftCard.Count);
                    CmdSelectDraftCard(GameManager.LocalPlayer.order, draftCard[rand]);
                    commander.Cancel();
                }
            })
            .Play();

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
                if(!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();
                GameManager.instance.ResetAcknowledgements();

                RpcEndSelectionDraftCard(GameManager.FirstOrder);
            })
            .OnUpdate(() => t -= Time.deltaTime)
            .Play();
    }

    [ClientRpc]
    void RpcEndSelectionDraftCard(int _firstOrder)
    {
        Commander commander = new();
        commander
            .Add(() =>
            {
                UIManager.Message.ForcePopUp("ī�� ������ ���ƽ��ϴ�!", 3f);
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(GameManager.GetPlayer(_firstOrder).ClientStartTurn)
            .Play();
    }

    #endregion
}
