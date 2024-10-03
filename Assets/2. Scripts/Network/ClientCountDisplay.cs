using UnityEngine;
using TMPro;
using Mirror;

public class ClientCountDisplay : NetworkBehaviour
{
    public TextMeshProUGUI clientCountText; // 연결된 클라이언트 수를 표시할 Text UI

    NetworkManager manager;

    // SyncVar로 클라이언트와 동기화할 플레이어 수
    [SyncVar] private int connectedPlayerCount;

    private void Start()
    {
        clientCountText = GetComponent<TextMeshProUGUI>();
        manager = NetworkManager.singleton;

        // 서버일 경우에만 플레이어 수 업데이트 시작
        if (isServer)
        {
            InvokeRepeating(nameof(UpdatePlayerCount), 1f, 1f); // 1초마다 업데이트
        }
    }

    // 서버에서 실행 - 현재 연결된 플레이어 수 업데이트
    [Server]
    private void UpdatePlayerCount()
    {
        connectedPlayerCount = manager.numPlayers;
    }

    private void Update()
    {
        // 클라이언트와 서버 모두에서 업데이트할 수 있도록 함
        UpdateClientCount();
    }

    // 현재 연결된 클라이언트 수를 업데이트하는 메서드
    void UpdateClientCount()
    {
        clientCountText.text = $"( {connectedPlayerCount} / 4 )";
    }
}
