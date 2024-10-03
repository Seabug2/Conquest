using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AssignAuthorityToClient : NetworkBehaviour
{
    NetworkIdentity identity;
    NetworkIdentity Identity
    {
        get
        {
            if(identity == null)
            {
                identity = GetComponent<NetworkIdentity>();
            }
            return identity;
        }
    }
    public override void OnStartClient()
    {
        CmdRequestSync();
    }

    // 서버가 클라이언트로 데이터 전송 (권한 필요 없음)
    [TargetRpc]
    public void TargetSendData(NetworkConnection target, string message)
    {
        Debug.Log($"클라이언트로 데이터 전송: {message}");
    }

    // 클라이언트가 데이터 요청
    [Command(requiresAuthority = false)]
    public void CmdRequestSync()
    {
        Debug.Log("서버에 동기화 요청 받음.");

        // TargetRpc를 통해 클라이언트로 데이터 전송 (권한 없이 사용 가능)
        TargetSendData(connectionToClient, "서버로부터 전송된 메시지");
    }
}
