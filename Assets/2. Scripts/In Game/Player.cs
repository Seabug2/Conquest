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

    #region ����
    //Ŭ���̾�Ʈ�� �����Ǿ��� ��
    public override void OnStartClient()
    {
        //���� �Ŵ����� �÷��̾� ����Ʈ�� �߰�
        GameManager.instance.AddPlayer(this);
    }

    //���� �÷��̾� ��ü�μ� Ŭ���̾�Ʈ�� �����Ǿ��� ��
    public override void OnStartLocalPlayer()
    {

    }
    #endregion

    public Hand Hand => GameManager.dict_Hand[order];
    public Field Field => GameManager.dict_Field[order];

    //�ڽ��� ����
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

            //��ο�
            //CmdDraw()
        }
        else
        {

        }
    }

    //��ο�
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
            //ī�带 ��ο� �� ��,
            //���� �ڽ��� �п� �� �� �ִ� ī�尡 �ִٸ� => CmdHandling();
            //Ȥ��, �ڽ��� �п� �� �� �ִ� ī�尡 ���ٸ� => CmdEndTurn();
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

    #region ����
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
