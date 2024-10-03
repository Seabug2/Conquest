using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    readonly Commander commander = new();
    #region �ʱ�ȭ
    public int Order { get; private set; }

    private void OnDestroy()
    {
        commander.Cancel();
    }

    public void SetOrder(int @new)
    {
        gameObject.name = $"Player_{@new}";
        Order = @new;
    }

    [ClientRpc]
    public void RpcSetOrder(int @new)
    {
        SetOrder(@new);

        if (isLocalPlayer)
        {
            GameManager.instance.CmdReply(@new);
        }
    }
    #endregion


    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver = false;
    void PlayerGameOver(bool _, bool @new)
    {
        if (!@new) return;

        if (isLocalPlayer)
        {
            UIManager.GetUI<HeadLine>().ForcePrint("�й�");
        }
        else
        {

        }
    }

    public bool isMyTurn = false;
    public bool hasTurn = false;

    #region ����
    //Ŭ���̾�Ʈ�� �����Ǿ��� ��
    void Start()
    {
        //������ ���� ����
        if (GameManager.instance != null && GameManager.instance.CurrentPhase.Equals(GamePhase.Standby))
        {
            GameManager.instance.AddPlayer(this);
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("�̹� ������ ���� �Ǿ����ϴ�. ���� ����");
        }
    }
    #endregion

    public Hand Hand => GameManager.dict_Hand[Order];
    public Field Field => GameManager.dict_Field[Order];

    [ClientRpc]
    public void RpcStartTurn()
    {
        ClientStartTurn();
    }

    [Client]
    public void ClientStartTurn()
    {
        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;

        commander
            .Refresh()
            .WaitWhile(isPlaying)
            .Add(() =>
            {
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.MoveLock(true);
                string message = isLocalPlayer ? "����� �����Դϴ�" : $"{Order + 1}��° �÷��̾��� �����Դϴ�";
                UIManager.GetUI<LineMessage>().ForcePopUp(message, 3f);
            }, 3f)
            .Add(StartDrawPhase)
            .Play();
    }
    [Client]
    void StartDrawPhase()
    {
        if (isLocalPlayer) CmdStartDrawPhase();
    }
    //��ο�
    [Command]
    public void CmdStartDrawPhase()
    {
        int drawnCardID = GameManager.Deck.DrawCardID();

        RpcStartDrawPhase(drawnCardID);
    }
    [ClientRpc]
    public void RpcStartDrawPhase(int id)
    {
        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;

        commander
            .Refresh()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("��ο�!", 2f))
            .WaitSeconds(1f)
            .Add(() =>
            {
                Card drawnCard = GameManager.Card(id);
                drawnCard.iCardState = isLocalPlayer ? new InHandState(drawnCard, Hand) : GameManager.instance.noneState;
                drawnCard.IsOpened = isLocalPlayer;
                Hand.Add(drawnCard);

                StartMainPhase();
            })
            .Play();
    }

    [Client]
    void StartMainPhase()
    {
        if (isLocalPlayer)
        {
            Handling();
        }
        else
        {
            Waiting();
        }
    }

    [Client]
    public void Handling()
    {
        commander.Refresh();

        if (Hand.Count == 0)
        {
            commander
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f), 2f)
                .Add(() => CmdEndTurn(false))
                .Play();
        }
        else
        {
            commander
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f), 2f)
                .Add(() =>
                {
                    Hand.SetHandlingState(Field);
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().Play(30f, () => ClientEndTurn());
                })
                .Play();
        }
    }

    [Client]
    public void Waiting()
    {
        commander
            .Refresh()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}��\n���� ���� ���� ���Դϴ�.", 2f)
            , 2f)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);
                UIManager.GetUI<Timer>().Play(30f);
            })
            .Play();
    }

    [Client]
    public void ClientEndTurn()
    {
        if (UIManager.GetUI<Timer>().IsPlaying())
            UIManager.GetUI<Timer>().Stop();

        CameraController.instance.Raycaster.eventMask = -1;

        Field.ShowPlaceableTiles(null, false);
        Hand.SetHandlingState();
        CmdEndTurn(Hand.IsLimitOver);
    }

    [Command]
    public void CmdEndTurn(bool isGameOver)
    {
        if (isGameOver)
        {
            this.isGameOver = isGameOver;
            GameManager.Deck.CmdReturnCard(Hand.AllIDs, true);
            GameManager.Deck.CmdReturnCard(Field.AllIDs, true);
            Hand.CmdRemoveAll();
        }

        RpcEndTurn(isGameOver);
    }

    [ClientRpc]
    public void RpcEndTurn(bool isGameOver)
    {
        UIManager.GetUI<Timer>().Reset();
        CameraController.instance.FocusOnPlayerField(Order);
        CameraController.instance.MoveLock(true);

        Hand.HandAlignment();

        string message = isGameOver ? $"�÷��̾� {Order + 1}�� Ż���߽��ϴ�!" : $"�÷��̾� {Order + 1}��\n���ʸ� ��Ĩ�ϴ�!";
        UIManager.GetUI<LineMessage>().ForcePopUp(message, 2f);

        if (isLocalPlayer)
        {
            CmdNextTurn(Order);
        }
    }

    [Command]
    public void CmdNextTurn(int order)
    {
        GameManager.instance.EndTurn(order);
    }

    #region ����
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
