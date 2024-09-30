using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    readonly Commander cmd = new();
    #region 초기화
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
        cmd.Reset()
            .Add(() =>
            {
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.MoveLock(true);
                if (isLocalPlayer)
                {
                    UIManager.GetUI<HeadLine>().Print("당신의 차례");
                    UIManager.GetUI<LineMessage>().ForcePopUp("당신의 차례입니다", 3f);
                }
                else
                {
                    UIManager.GetUI<HeadLine>().Print($"플레이어 {Order + 1}의 차례");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{Order + 1}번째 플레이어의 차례입니다", 3f);
                    cmd.Cancel();
                }
            }, 3f)
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
        UIManager.GetUI<LineMessage>().ForcePopUp("드로우!", 2f);

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
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("나쁜 짓을 할 시간입니다!", 2f)
                , 2f)
                .Add(() =>
                {
                    //버튼에 이벤트를 새로 등록?

                    CameraController.instance.MoveLock(false);

                    if (Hand.Count < 1)
                    {
                        UIManager.GetUI<LineMessage>().ForcePopUp("사용 가능한 카드가 없습니다!", 2f);

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
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}가\n카드를 고르는 중입니다.", 2f)
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
            UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}가 탈락했습니다!", 2f);
            UIManager.GetUI<HeadLine>().ForcePrint("패배");
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}의\n차례를 마칩니다!", 2f);
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

    #region 종료
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
