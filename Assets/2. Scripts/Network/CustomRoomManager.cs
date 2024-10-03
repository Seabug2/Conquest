using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomRoomManager : NetworkRoomManager
{
    //룸매니저로 생성된 오브젝트들은 offline 씬으로 이동하면 자동으로 삭제된다.

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
    }
}
