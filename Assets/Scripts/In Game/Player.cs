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

    [SyncVar, SerializeField, Header("���� ���� ����")]
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
        //�޽��� ��� "~�� ��" ���� �� Draw() ����
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
        //�޽��� ��� "~�� ��ο�" ���� �Ŀ� StartHandlingPhase() ����
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

    //���� �÷��̾��� �� �Ŵ������� ȣ��
    [Command]
    public void CmdTurnEnd()
    {
        hasTurn = false;

        RpcTurnEnd();
    }

    //��� Ŭ���̾�Ʈ���� ȣ��
    [ClientRpc]
    public void RpcTurnEnd()
    {
        //���ʸ� ��ġ�� UI �˾�


        if (GameManager.RoundFinished)
        {
            GameManager.Deck.ServerDraftPhase();
            return;
        }

        //���� ������ �÷��̾�

        if (isLocalPlayer)
        {

        }
        else
        {

        }
    }
}
