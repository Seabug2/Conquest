using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomRoomManager : NetworkRoomManager
{
    //��Ŵ����� ������ ������Ʈ���� offline ������ �̵��ϸ� �ڵ����� �����ȴ�.

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
    }
}
