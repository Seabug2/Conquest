using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    readonly Commander cmd = new();
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

            SideMenu menu = UIManager.GetUI<SideMenu>();
            menu.Buttons[@new].onClick.RemoveAllListeners();
            menu.Buttons[@new].onClick.AddListener(OnClickMyButton);

            menu.SetLocalButton(@new);
        }
    }
    #endregion

    void OnClickMyButton()
    {
        if (GameManager.instance.CurrentPhase.Equals(GamePhase.DraftPhase))
        {
            if (CameraController.instance.CurrentCamIndex == Order)
                CameraController.instance.FocusOnCenter();
            else
                CameraController.instance.FocusOnHome();
        }
        else if (GameManager.instance.CurrentPhase.Equals(GamePhase.PlayerPhase))
        {
            if (CameraController.instance.CurrentCamIndex == Order)
            {
                if (isMyTurn)
                {
                    UIManager.GetUI<Timer>().Stop();
                    Hand.SetHandlingState(null);
                    CmdEndTurn(Hand.IsLimitOver);
                }
            }
            else
            {
                CameraController.instance.FocusOnHome();
            }
        }
    }

    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver = false;
    void PlayerGameOver(bool _, bool @new)
    {
        if (isLocalPlayer)
        {
            if (@new)
            {

            }
            else
            {

            }
        }
    }

    public bool isMyTurn = false;
    public bool hasTurn = false;

    #region ����
    //Ŭ���̾�Ʈ�� �����Ǿ��� ��
    void Start()
    {
        //���� �Ŵ����� �÷��̾� ����Ʈ�� �߰�
        GameManager.instance.AddPlayer(this);
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
        cmd.Reset()
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
                    cmd.Cancel();
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

        cmd.Reset()
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
            cmd.Reset()
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f)
                , 2f)
                .Add(() =>
                {
                    //��ư�� �̺�Ʈ�� ���� ���?

                    CameraController.instance.MoveLock(false);

                    if (Hand.Count < 1)
                    {
                        UIManager.GetUI<LineMessage>().ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);

                        Hand.SetHandlingState(null);
                        CmdEndTurn(Hand.IsLimitOver);
                    }
                    else
                    {
                        Hand.SetHandlingState(Field);
                    }

                    UIManager.GetUI<Timer>().Play(30f, () =>
                    {
                        Hand.SetHandlingState(null);
                        CmdEndTurn(Hand.IsLimitOver);
                    });
                })
                .Play();
        }
        else
        {
            cmd.Reset()
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}��\nī�带 ���� ���Դϴ�.", 2f)
                , 2f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().Play(30f);
                })
                .Play();
        }
    }

    [Command]
    public void CmdEndTurn(bool isGameOver)
    {
        this.isGameOver = isGameOver;

        if (isGameOver)
        {
            GameManager.deck.CmdReturnCard(Hand.AllIDs, true);
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

        cmd.Reset()
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
