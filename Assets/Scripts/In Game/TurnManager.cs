using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class TurnManager : NetworkBehaviour
{
    public UnityEvent @event;

    public void StartTurn()
    {
        //메시지 출력 "~의 턴" 종료 후 Draw() 실행
        if (isLocalPlayer)
        {

        }
        else
        {

        }
    }

    void Draw()
    {
        //메시지 출력 "~의 드로우" 종료 후에 StartHandlingPhase() 실행
        if (isLocalPlayer)
        {

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
}
