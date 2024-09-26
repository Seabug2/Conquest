using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        {
            GameManager.instance.CmdReply(@new);
        }
    }

    [SyncVar(hook = nameof(PlayerGameOver))]
    public bool isGameOver = false;
    void PlayerGameOver(bool _, bool @new)
    {
        if (isLocalPlayer)
        {
            //CmdAcknowledgeToManager(order);
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
                    UIManager.Message.ForcePopUp($"{order + 1}��° �÷��̾��� �����Դϴ�", 3f);
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdStartDrawPhase)
            .Play();
    }

    //��ο�
    [Command]
    public void CmdStartDrawPhase()
    {
        int drawnCardID = GameManager.deck.DrawCardID();

        RpcStartDrawPhase(drawnCardID);
    }

    [ClientRpc]
    public void RpcStartDrawPhase(int id)
    {
        Commander commander = new();
        commander
            .Add(() => UIManager.Message.ForcePopUp("��ο�!", 2f), 1f)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    Hand.CmdAdd(id);
                }
                else
                {
                    commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(CmdHandling)
            .Play();
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
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    UIManager.Message.ForcePopUp("���� ���� �� �ð��Դϴ�!", 2f);
                }
                else
                {
                    UIManager.Message.ForcePopUp($"�÷��̾� {order + 1}��\nī�带 ���� ���Դϴ�.", 2f);
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);

                if (isLocalPlayer)
                {
                    //TODO �� Ȯ�� �����
                    /*
                     * if(...)
                     *�÷��̾��� �и� Ȯ���Ͽ� �� �� �ִ� ī�尡 ���ٸ� �ٷ� ���ʸ� �����մϴ�.
                     */
                    UIManager.Message.ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);
                }
                else
                {
                    Commander.Cancel();
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(() =>
            {
                //TODO ���⿡ �տ� �� ī���� ���� üũ �� Ż���� ���� �߰�

                CmdEndTurn(false);
            })
            .Play();
    }

    [Command]
    public void CmdEndTurn(bool isGameOver)
    {
        //�������� isGameOver�� �����ؾ���
        this.isGameOver = isGameOver;
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
                   UIManager.Message.ForcePopUp($"�÷��̾� {order + 1}��\n���ʸ� ��Ĩ�ϴ�!", 2f);

                   if (!isLocalPlayer)
                   {
                       Commander.Cancel();
                   }
               })
               .WaitWhile(UIManager.Message.IsPlaying)
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
