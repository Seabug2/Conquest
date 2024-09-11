using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public  enum Phase
{
    myTurn,
    otherTurn,
    draftPhase
}

public class TurnManager : MonoBehaviour
{
    /* 
     * GamaManager에서 TurnManager에게 
     * n번째 플레이어의 차례를 실행하라고 명령을 내리면
     * TurnManager가 플레이어의 차례동안 작업을 한다...
     * 
     * 자신의 차례에만 실행?
     */

    public void StartSelectPhase()
    {
        if (SelectPhase != null)
        {
            StopCoroutine(SelectPhase);
        }
        SelectPhase = StartCoroutine(SelectPhase_co());
    }

    Coroutine SelectPhase;

    IEnumerator SelectPhase_co()
    {
        /*
         //만약 플레이어의 패가 없거나 패에 낼 수 있는 카드가 없으면 차례를 마친다.
         */

        while (true)
        {
            yield return null;
        }
    }

    IEnumerator EndTurn()
    {
        yield break;
    }
}
