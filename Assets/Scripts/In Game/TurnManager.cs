using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
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
