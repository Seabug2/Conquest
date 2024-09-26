using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(Hook_SetOrder))]
    public int order = -1;

    void Hook_SetOrder(int _, int @new)
    {
        gameObject.name = $"Player_{@new}";

        if (isLocalPlayer)
        {
            GameManager.instance.Reply(@new);
        }
    }

    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver = false;
    void PlayerGameOver(bool _, bool @new)
    {
        if (isLocalPlayer)
        {
            //CmdAcknowledgeToManager(order);
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

    public Hand Hand => GameManager.dict_Hand[order];
    public Field Field => GameManager.dict_Field[order];

    //자신의 차례
    [Command]
    public void CmdStartTurn()
    {
        RpcStartTurn();
    }

    [ClientRpc]
    public void RpcStartTurn()
    {
        ClientStartTurn();
    }

    [Client]
    public void ClientStartTurn()
    {
        Commander commander = new();
        commander
            .Add(() =>
            {
                CameraController.instance.CurrentCamIndex = order;
                CameraController.instance.MoveLock(true);
                if (isLocalPlayer)
                {
                    UIManager.Message.ForcePopUp("당신의 차례입니다", 3f);
                }
                else
                {
                    UIManager.Message.ForcePopUp($"{order}번째 플레이어의 차례입니다", 3f);
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdDraw).Play();

    }

    //드로우
    [Command]
    public void CmdDraw()
    {
        int drawnCardID = GameManager.deck.DrawCardID();

        RpcDraw(drawnCardID);
    }

    [ClientRpc]
    public void RpcDraw(int id)
    {
        Commander Commander = new Commander()
            .Add(() => UIManager.Message.ForcePopUp("드로우!", 2f), 1f)
            .Add(() => Hand.CmdAdd(id))
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    CmdHandling();
                }
            });

        Commander.Play();
    }

    [Command]
    public void CmdHandling()
    {
        RpcHandling();
    }

    [ClientRpc]
    public void RpcHandling()
    {
        Commander Commander = new();
        Commander
            .Add(() => UIManager.Message.ForcePopUp("나쁜 짓을 할 시간입니다!", 2f), 2f)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);

                if (isLocalPlayer)
                {
                    UIManager.Message.ForcePopUp("사용 가능한 카드가 없습니다!", 2f);
                }
                else
                {
                    //TODO 패 확인 만들기
                    /*
                     * if(...)
                     *플레이어의 패를 확인하여 낼 수 있는 카드가 없다면 바로 차례를 종료합니다.
                     */
                    Commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdEndTurn).Play();
    }

    [Command]
    public void CmdEndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn()
    {
        Commander Commander = new();
        Commander
               .Add(() =>
               {
                   CameraController.instance.CurrentCamIndex = order;
                   CameraController.instance.MoveLock(true);
                   UIManager.Message.ForcePopUp("차례를 마칩니다!", 2f);

                   if (!isLocalPlayer)
                   {
                       Commander.Cancel();
                   }
               }, 2f)
               .Add(CmdNextTurn)
               .Play();
    }

    [Command]
    public void CmdNextTurn()
    {
        GameManager.instance.EndTurn();
    }

    #region 종료
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
