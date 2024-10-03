using UnityEngine;
using Mirror;

public class DisconnectButtonHandler : MonoBehaviour
{
    // 버튼 클릭 시 호출되는 메서드
    public void OnDisconnectButtonClick()
    {
        if (NetworkServer.active && NetworkClient.active)
        {
            // 호스트인 경우
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            // 클라이언트인 경우
            NetworkManager.singleton.StopHost();
        }
    }
}
