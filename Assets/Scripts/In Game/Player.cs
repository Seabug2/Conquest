using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class Player : NetworkBehaviour
{
    [SyncVar]
    public int order = -1;

    public bool IsLocalPlayer => IsLocalPlayer;
    public bool IsMyTurn => GameManager.instance.CurrentOrder.Equals(order);

    [SyncVar, SerializeField, Header("게임 오버 상태")]
    public bool isGameOver = false;
    public bool isTurnSkipped = false;
    public bool isCanDraw = false;

    [SyncVar]
    public bool hasTurn = false;


    public override void OnStartClient()
    {
        GameManager.instance.AddPlayer(this);
    }

    public Hand hand;
    public Field field;

    [Command]
    public void CmdStartTurn()
    {
        RpcStartTurn();
    }

    [ClientRpc]
    public void RpcStartTurn()
    {
        //메시지 출력 "~의 턴" 종료 후 Draw() 실행
        if (isLocalPlayer)
        {

            return;
        }
        else
        {

        }
    }


    [Command]
    void CmdDraw()
    {
        RpcDraw();
    }

    [ClientRpc]
    void RpcDraw()
    {
        //메시지 출력 "~의 드로우" 종료 후에 StartHandlingPhase() 실행
        if (!isLocalPlayer)
        {
            return;
        }
        else
        {

        }
    }

    void StartHandlingPhase()
    {
        if (isLocalPlayer)
        {

        }
        else
        {

        }
    }

    Coroutine HandlingPhase;

    IEnumerator HandlingPhase_co()
    {
        //

        while (true)
        {
            yield return null;
        }
    }

    //로컬 플레이어의 턴 매니저에서 호출
    [Command]
    public void CmdTurnEnd()
    {
        hasTurn = false;

        RpcTurnEnd();
    }

    //모든 클라이언트에서 호출
    [ClientRpc]
    public void RpcTurnEnd()
    {
        //차례를 마치는 UI 팝업


        if (GameManager.RoundFinished)
        {
            GameManager.Deck.ServerDraftPhase();
            return;
        }

        //다음 차례의 플레이어

        if (isLocalPlayer)
        {

        }
        else
        {

        }
    }
}
