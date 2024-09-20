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
            //�������� ���� ī�� �� ���� �޾ƿ�
            int drawnCard = GameManager.Deck.DrawCardID();
            //��ο츦 �ϴ� ������ ���ʸ� ����...
            RpcStartTurn(drawnCard);
        }
    }

    [ClientRpc]
    void RpcStartTurn(int _cardID)
    {
        Commander commander = new Commander();
        commander
            .Add_While(() => UIMaster.Message.ForcePopUp($"{order + 1}�� ����!", 2f), UIMaster.Message.IsPlaying)
            .Add_While(() => UIMaster.Message.ForcePopUp($"{order + 1}�� ��ο�!", 2f), UIMaster.Message.IsPlaying)
            .Add(() => hand.Add(_cardID), 1f)
            .Add(() =>
            {
                if (isLocalPlayer)
                {
                    hand.HandSetting(field);
                    Debug.Log("���ʸ� ����...");
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
        //�޽��� ��� "~�� ��ο�" ���� �Ŀ� StartHandlingPhase() ����
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


    //���� �÷��̾��� �� �Ŵ������� ȣ��
    [Command]
    public void CmdTurnEnd()
    {
        isGameOver = hand.IsLimitOver;
        GameManager.instance.NextTurn(order);
    }

    //��� Ŭ���̾�Ʈ���� ȣ��
    [ClientRpc]
    public void RpcTurnEnd()
    {

    }
}
