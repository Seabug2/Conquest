using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    [Command]
    private void CmdAcknowledgeToManager()
    {
        GameManager.instance.Reply(order);
    }

    [SyncVar(hook = nameof(Hook_SetOrder))]
    public int order = -1;

    void Hook_SetOrder(int _, int @new)
    {
        gameObject.name = $"Player_{@new}";

        if (isLocalPlayer)
            CmdAcknowledgeToManager();
    }

    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver;
    void PlayerGameOver(bool _, bool @new)
    {
        if (isLocalPlayer)
        {
            CmdAcknowledgeToManager();
            if (@new)
            {

            }
            else
            {

            }
        }
    }

    public bool isMyTurn = false;
    public bool hasTurn = false;

    #region ����
    //Ŭ���̾�Ʈ�� �����Ǿ��� ��
    void Start()
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
        ClientStartTurn();
    }

    [Client]
    public void ClientStartTurn()
    {
        Commander commander = new();
        commander
            .Add(() =>
            {
                CameraController.instance.CurrentCamIndex = order;
                CameraController.instance.MoveLock(true);
                if (isLocalPlayer)
                {
                    UIManager.Message.ForcePopUp("����� �����Դϴ�", 3f);
                }
                else
                {
                    UIManager.Message.ForcePopUp($"{order}��° �÷��̾��� �����Դϴ�", 3f);
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdDraw).Play();

    }

    //��ο�
    [Command]
    public void CmdDraw()
    {
        int drawnCardID = GameManager.deck.DrawCardID();

        RpcDraw(drawnCardID);
    }

    [ClientRpc]
    public void RpcDraw(int id)
    {
        Commander Commander = new Commander()
            .Add(() => UIManager.Message.ForcePopUp("��ο�!", 2f), 1f)
            .Add(() => Hand.CmdAdd(id))
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    CmdHandling();
                }
            });

        Commander.Play();
    }

    [Command]
    public void CmdHandling()
    {
        RpcHandling();
    }

    [ClientRpc]
    public void RpcHandling()
    {
        Commander Commander = new();
        Commander
            .Add(() => UIManager.Message.ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f), 2f)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);

                if (isLocalPlayer)
                {
                    UIManager.Message.ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);
                }
                else
                {
                    //TODO �� Ȯ�� �����
                    /*
                     * if(...)
                     *�÷��̾��� �и� Ȯ���Ͽ� �� �� �ִ� ī�尡 ���ٸ� �ٷ� ���ʸ� �����մϴ�.
                     */
                    Commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdEndTurn).Play();
    }

    [Command]
    public void CmdEndTurn()
    {
        RpcEndTurn();
    }

    [ClientRpc]
    public void RpcEndTurn()
    {
        Commander Commander = new();
        Commander
               .Add(() =>
               {
                   CameraController.instance.CurrentCamIndex = order;
                   CameraController.instance.MoveLock(true);
                   UIManager.Message.ForcePopUp("���ʸ� ��Ĩ�ϴ�!", 2f);

                   if (!isLocalPlayer)
                   {
                       Commander.Cancel();
                   }
               }, 2f)
               .Add(CmdNextTurn)
               .Play();
    }

    [Command]
    public void CmdNextTurn()
    {
        GameManager.instance.EndTurn();
    }

    #region ����
    public override void OnStopClient()
    {
        GameManager.instance.RemovePlayer(this);
    }
    #endregion 
}
