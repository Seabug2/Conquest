using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    readonly Commander commander = new();
    #region 초기화
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
            UIManager.GetUI<HeadLine>().ForcePrint("패배");
        }
        else
        {

        }
    }

    public bool isMyTurn = false;
    public bool hasTurn = false;

    #region 생성
    //클라이언트에 생성되었을 때
    void Start()
    {
        //게임이 아직 시작
        if (GameManager.instance != null && GameManager.instance.CurrentPhase.Equals(GamePhase.Standby))
        {
            GameManager.instance.AddPlayer(this);
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("이미 게임이 시작 되었습니다. 연결 종료");
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
                //차례를 시작한 플레이어의 화면으로 이동하고
                //다른 화면으로 이동하지 못하게 막는다.
                CameraController.instance.FocusOnPlayerField(Order);
                CameraController.instance.Freeze(true);
                string message = isLocalPlayer ? "당신의 차례입니다" : $"{Order + 1}번째 플레이어의 차례입니다";
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
    //드로우
    [Command]
    public void CmdStartDrawPhase()
    {
        int drawnCardID = GameManager.Deck.DrawCardID();

        RpcStartDrawPhase(drawnCardID);
    }
    [ClientRpc]
    public void RpcStartDrawPhase(int id)
    {
        commander
            .Refresh()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("드로우!", 2f))
            .WaitSeconds(1f)
            .Add(() =>
            {
                Card drawnCard = GameManager.Card(id);
                drawnCard.SprtRend.sortingLayerName = "Default";
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
                .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("사용 가능한 카드가 없습니다!", 2f), 2f)
                .Add(() => CmdEndTurn(false))
                .Play();
        }
        else
        {
            commander
                .WaitWhile(UIManager.GetUI<LineMessage>().IsPlaying)
                .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("나쁜 짓을 할 시간입니다!", 2f), 2f)
                .Add(() =>
                {
                    Hand.SetHandlingState(Field);
                    CameraController.instance.Freeze(false);
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
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp($"플레이어 {Order + 1}가\n나쁜 짓을 생각 중입니다.", 2f)
            , 2f)
            .Add(() =>
            {
                CameraController.instance.Freeze(false);
                UIManager.GetUI<Timer>().Play(30f);
            })
            .Play();
    }

    [Client]
    public void ClientEndTurn()
    {
        if (UIManager.GetUI<Timer>().IsPlaying())
            UIManager.GetUI<Timer>().Stop();

        Field.ShowPlaceableTiles(null, false);

        CameraController.instance.Freeze(true);
        Hand.SetHandlingState();
        
        if (isLocalPlayer)
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
        CameraController.instance.Freeze(true);
        Hand.HandAlignment();


        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;
        string message = isGameOver ? $"플레이어 {Order + 1}가 탈락했습니다!" : $"플레이어 {Order + 1}의\n차례를 마칩니다!";
        commander
            .Refresh()
            .WaitWhile(isPlaying)
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp(message, 2f)
            , 2.2f)
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
        hasTurn = true;
        isMyTurn = false;
        GameManager.instance.EndTurn(order);
    }

    #region 종료
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
