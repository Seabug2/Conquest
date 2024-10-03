using UnityEngine;
using TMPro;
using Mirror;

public class ClientCountDisplay : NetworkBehaviour
{
    public TextMeshProUGUI clientCountText; // ����� Ŭ���̾�Ʈ ���� ǥ���� Text UI

    NetworkManager manager;

    // SyncVar�� Ŭ���̾�Ʈ�� ����ȭ�� �÷��̾� ��
    [SyncVar] private int connectedPlayerCount;

    private void Start()
    {
        clientCountText = GetComponent<TextMeshProUGUI>();
        manager = NetworkManager.singleton;

        // ������ ��쿡�� �÷��̾� �� ������Ʈ ����
        if (isServer)
        {
            InvokeRepeating(nameof(UpdatePlayerCount), 1f, 1f); // 1�ʸ��� ������Ʈ
        }
    }

    // �������� ���� - ���� ����� �÷��̾� �� ������Ʈ
    [Server]
    private void UpdatePlayerCount()
    {
        connectedPlayerCount = manager.numPlayers;
    }

    private void Update()
    {
        // Ŭ���̾�Ʈ�� ���� ��ο��� ������Ʈ�� �� �ֵ��� ��
        UpdateClientCount();
    }

    // ���� ����� Ŭ���̾�Ʈ ���� ������Ʈ�ϴ� �޼���
    void UpdateClientCount()
    {
        clientCountText.text = $"( {connectedPlayerCount} / 4 )";
    }
}
