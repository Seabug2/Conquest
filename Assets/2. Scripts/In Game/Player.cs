using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
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
            UIManager.GetUI<SideMenu>().Buttons[@new].onClick.RemoveAllListeners();
            UIManager.GetUI<SideMenu>().Buttons[@new].onClick.AddListener(OnClickMyButton);
        }
    }

    public void OnClickMyButton()
    {
        if (CameraController.instance.CurrentCamIndex == Order)
        {
            //ī�޶� �ڽ��� �ʵ带 �ٶ󺸴� ���̸鼭...
            if (isMyTurn && GameManager.instance.CurrentPhase.Equals(GamePhase.PlayerPhase))
            {
                UIManager.GetUI<Timer>().Stop();
            }
            else
            {
                CameraController.instance.FocusOnCenter();
            }
        }
        else
        {
            //ī�޶� �ٸ� ���� �ٶ󺸰� ������ �ڽ��� �ʵ带 �ٶ󺻴�.
            CameraController.instance.FocusOnHome();
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
        Commander commander = new();
        commander
            .Add(() =>
            {
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.MoveLock(true);
                if (isLocalPlayer)
                {
                    UIManager.GetUI<LineMessage>().ForcePopUp("����� �����Դϴ�", 3f);
                }
                else
                {
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{Order + 1}��° �÷��̾��� �����Դϴ�", 3f);
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
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
        Commander commander = new();
        commander
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("��ο�!", 2f), 1f)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    Hand.CmdAdd(id);
                }
                else
                {
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
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
            Commander Commander = new();
            Commander
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print("����� ����");
                    UIManager.GetUI<LineMessage>().ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f);
                }, 2f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().SetTimer(60f);

                    if (Hand.Count < 1)
                    {
                        UIManager.GetUI<LineMessage>().ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);
                        Commander.Cancel();
                    }
                    else
                    {
                        Hand.SetHandlingState(Field);
                    }
                })
                .WaitWhile(() => UIManager.GetUI<Timer>().IsPlaying)
                .OnCanceled(() =>
                {
                    CmdEndTurn(Hand.IsLimitOver);
                })
                .Play();
        }
        else
        {
            new Commander()
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print($"�÷��̾� {Order + 1}�� ����");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}��\nī�带 ���� ���Դϴ�.", 2f);
                })
                .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().SetTimer(60f);
                })
                .Play();
        }
    }

    [Command]
    public void CmdEndTurn(bool isGameOver)
    {
        //�������� isGameOver�� �����ؾ���
        this.isGameOver = isGameOver;
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn()
    {
        new Commander()
            .Add(() =>
            {
                UIManager.GetUI<Timer>().Reset();
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.MoveLock(true);
                UIManager.GetUI<LineMessage>().ForcePopUp($"�÷��̾� {Order + 1}��\n���ʸ� ��Ĩ�ϴ�!", 2f);
            })
            .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    CmdNextTurn(Order);
                }
            })
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
