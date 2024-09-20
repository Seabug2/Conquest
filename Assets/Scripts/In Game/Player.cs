using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
public class Player : NetworkBehaviour
{
    [SyncVar]
    public int order = -1;
    [SyncVar]
    public bool isGameOver = false;
    [SyncVar]
    public bool hasTurn = false;

    public bool isTurnSkipped = false;
    public bool isCanDraw = false;

    public override void OnStartClient()
    {
        GameManager.instance.AddPlayer(this);
    }

    public Hand hand => GameManager.Hand(order);
    public Field field => GameManager.Field(order);




    [Command]
    public void CmdStartTurn()
    {
        if (isTurnSkipped)
        {

        }
        else
        {
            //서버에서 덱의 카드 한 장을 받아옴
            int drawnCard = GameManager.Deck.DrawCardID();
            //드로우를 하는 것으로 차례를 시작...
            RpcStartTurn(drawnCard);
        }
    }

    [ClientRpc]
    void RpcStartTurn(int _cardID)
    {
        Commander commander = new Commander();
        commander
            .Add_While(() => UIMaster.Message.ForcePopUp($"{order + 1}의 차례!", 2f), UIMaster.Message.IsPlaying)
            .Add_While(() => UIMaster.Message.ForcePopUp($"{order + 1}의 드로우!", 2f), UIMaster.Message.IsPlaying)
            .Add(() => hand.Add(_cardID), 1f)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    hand.HandSetting(field);
                    Debug.Log("차례를 시작...");
                }
            })
            .Play();

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
        if (isLocalPlayer)
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


    //로컬 플레이어의 턴 매니저에서 호출
    [Command]
    public void CmdTurnEnd()
    {
        isGameOver = hand.IsLimitOver;
        GameManager.instance.NextTurn(order);
    }

    //모든 클라이언트에서 호출
    [ClientRpc]
    public void RpcTurnEnd()
    {

    }
}
