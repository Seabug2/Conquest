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

    // ������ Ŭ���̾�Ʈ�� ������ ���� (���� �ʿ� ����)
    [TargetRpc]
    public void TargetSendData(NetworkConnection target, string message)
    {
        Debug.Log($"Ŭ���̾�Ʈ�� ������ ����: {message}");
    }

    // Ŭ���̾�Ʈ�� ������ ��û
    [Command(requiresAuthority = false)]
    public void CmdRequestSync()
    {
        Debug.Log("������ ����ȭ ��û ����.");

        // TargetRpc�� ���� Ŭ���̾�Ʈ�� ������ ���� (���� ���� ��� ����)
        TargetSendData(connectionToClient, "�����κ��� ���۵� �޽���");
    }
}
