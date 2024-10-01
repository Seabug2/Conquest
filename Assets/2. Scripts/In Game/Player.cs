using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    readonly Commander commander = new();
    #region �ʱ�ȭ
    public int Order { get; private set; }
    [Server]
    public void SetOrder(int @new)
    {
        gameObject.name = $"Player_{@new}";
        Order = @new;
    }

    [ClientRpc]
    public void RpcSetOrder(int @new)
    {
        gameObject.name = $"Player_{@new}";
        Order = @new;

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

    //���� �÷��̾� ��ü�μ� Ŭ���̾�Ʈ�� �����Ǿ��� ��
    public override void OnStartLocalPlayer()
    {

    }
    #endregion

    public Hand Hand => GameManager.dict_Hand[Order];
    public Field Field => GameManager.dict_Field[Order];

    //�ڽ��� ����
    [Command]
    public void CmdStartTurn()
    {
        RpcStartTurn();
    }

    [ClientRpc]
    public void RpcStartTurn()
    {
        UIManager.GetUI<Timer>().@Reset();
        ClientStartTurn();
    }

    [Client]
    public void ClientStartTurn()
    {
        /// <see cref="Deck.RpcEndSelectionDraftCard"/>�� ����Ͽ� ���
        commander
            .Refresh()
            .Add(() =>
            {
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.MoveLock(true);
                if (isLocalPlayer)
                {
                    UIManager.GetUI<HeadLine>().Print("����� ����");
                    UIManager.GetUI<LineMessage>().ForcePopUp("����� �����Դϴ�", 3f);
                }
                else
                {
                    UIManager.GetUI<HeadLine>().Print($"�÷��̾� {Order + 1}�� ����");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{Order + 1}��° �÷��̾��� �����Դϴ�", 3f);
                    commander.Cancel();
                }
            }, 3f)
            .Add(CmdStartDrawPhase)
            .Play();
    }

    //��ο�
    [Command]
    public void CmdStartDrawPhase()
    {
        int drawnCardID = GameManager.deck.DrawCardID();

        RpcStartDrawPhase(drawnCardID);
    }

    [ClientRpc]
    public void RpcStartDrawPhase(int id)
    {
        UIManager.GetUI<LineMessage>().ForcePopUp("��ο�!", 2f);

        if (!isLocalPlayer) return;

        commander
            .Refresh()
            .WaitSeconds(1f)
            .Add(() => Hand.CmdAdd(id), 1f)
            .Add(CmdHandling)
            .Play();
    }

    [Command]
    public void CmdHandling()
    {
        RpcHandling();
    }

    [ClientRpc]
    public void RpcHandling()
    {
        if (isLocalPlayer)
        {
            commander
                .Refresh()
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f), 2f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);

                    if (Hand.Count == 0)
                    {
                        UIManager.GetUI<LineMessage>().ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);
                        CmdEndTurn(Hand.IsLimitOver);
                    }
                    else
                    {
                        Hand.SetHandlingState(Field);
                        UIManager.GetUI<Timer>().Play(30f, () => ClientEndTurn());
                    }
                })
                .Play();
        }
        else
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
        if (!isMyTurn) return;

        this.isGameOver = isGameOver;

        if (isGameOver)
        {
            GameManager.deck.CmdReturnCard(Hand.AllIDs, true);
            GameManager.deck.CmdReturnCard(Field.AllIDs, true);

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

        if (isGameOver)
        {
            UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}�� Ż���߽��ϴ�!", 2f);
            UIManager.GetUI<HeadLine>().ForcePrint("�й�");
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}��\n���ʸ� ��Ĩ�ϴ�!", 2f);
        }

        if (!isLocalPlayer) return;

        commander
            .Refresh()
            .WaitSeconds(2f)
            .Add(() => CmdNextTurn(Order))
            .Play();
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
