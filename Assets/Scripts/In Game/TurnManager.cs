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
        //�޽��� ��� "~�� ��" ���� �� Draw() ����
        if (isLocalPlayer)
        {

        }
        else
        {

        }
    }

    void Draw()
    {
        //�޽��� ��� "~�� ��ο�" ���� �Ŀ� StartHandlingPhase() ����
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
