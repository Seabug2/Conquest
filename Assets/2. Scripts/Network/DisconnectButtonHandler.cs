using UnityEngine;
using Mirror;

public class DisconnectButtonHandler : MonoBehaviour
{
    // ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnDisconnectButtonClick()
    {
        if (NetworkServer.active && NetworkClient.active)
        {
            // ȣ��Ʈ�� ���
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            // Ŭ���̾�Ʈ�� ���
            NetworkManager.singleton.StopHost();
        }
    }
}
