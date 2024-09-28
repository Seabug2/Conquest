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
            //카메라가 자신의 필드를 바라보는 중이면서...
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
            //카메라가 다른 곳을 바라보고 있으면 자신의 필드를 바라본다.
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

    #region 생성
    //클라이언트에 생성되었을 때
    void Start()
    {
        //게임 매니저의 플레이어 리스트에 추가
        GameManager.instance.AddPlayer(this);
    }

    //로컬 플레이어 객체로서 클라이언트에 생성되었을 때
    public override void OnStartLocalPlayer()
    {

    }
    #endregion

    public Hand Hand => GameManager.dict_Hand[Order];
    public Field Field => GameManager.dict_Field[Order];

    //자신의 차례
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
                    UIManager.GetUI<LineMessage>().ForcePopUp("당신의 차례입니다", 3f);
                }
                else
                {
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{Order + 1}번째 플레이어의 차례입니다", 3f);
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
            .Add(CmdStartDrawPhase)
            .Play();
    }

    //드로우
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
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("드로우!", 2f), 1f)
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
                    UIManager.GetUI<HeadLine>().Print("당신의 차례");
                    UIManager.GetUI<LineMessage>().ForcePopUp("나쁜 짓을 할 시간입니다!", 2f);
                }, 2f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().SetTimer(60f);

                    if (Hand.Count < 1)
                    {
                        UIManager.GetUI<LineMessage>().ForcePopUp("사용 가능한 카드가 없습니다!", 2f);
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
                    UIManager.GetUI<HeadLine>().Print($"플레이어 {Order + 1}의 차례");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}가\n카드를 고르는 중입니다.", 2f);
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
        //서버에서 isGameOver를 변경해야함
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
                UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}의\n차례를 마칩니다!", 2f);
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

    #region 종료
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
