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
         //���� �÷��̾��� �а� ���ų� �п� �� �� �ִ� ī�尡 ������ ���ʸ� ��ģ��.
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
