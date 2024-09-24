using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
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
            GameManager.instance.Ackn_SortPlayerList(@new);
    }

    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver;
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

    [SyncVar] public bool isMyTurn;
    [SyncVar] public bool hasTurn;

    #region 생성
    //클라이언트에 생성되었을 때
    public override void OnStartClient()
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
        if (isLocalPlayer)
        {
            GameManager.instance.currentOrder = order;

            //드로우
            //CmdDraw()
        }
        else
        {

        }
    }

    //드로우
    [Command]
    public void CmdDraw()
    {
        RpcDraw();
    }

    [ClientRpc]
    public void RpcDraw()
    {
        if (isLocalPlayer)
        {
            //카드를 드로우 한 후,
            //만약 자신의 패에 낼 수 있는 카드가 있다면 => CmdHandling();
            //혹은, 자신의 패에 낼 수 있는 카드가 없다면 => CmdEndTurn();
        }
        else
        {

        }
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

        }
        else
        {

        }
    }

    [Command]
    public void CmdEndTurn()
    {
        GameManager.instance.EndTurn();
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn()
    {

    }

    #region 종료
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
