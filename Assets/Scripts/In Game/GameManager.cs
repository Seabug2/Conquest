using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //���� Ŭ���� ����
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    [Header("ī�� �޸� �̹���"), Space(10)]
    public Sprite cardBackFace;

    [SerializeField]
    [Header("�÷��̾� ������Ʈ"), Space(10)]
    List<NetworkPlayer> players = new List<NetworkPlayer>();
    public NetworkPlayer GetPlayer(int i)
    {
        return players[i];
    }

    [Header("�÷��̾� ������Ʈ�� ���� ���"), Space(10)]
    public CardManager cardManager;
    public Field[] fields;
    public Hand[] hands;

    #region #1 �÷��̾��� ����� ���� ����
    //�� Ŭ���̾�Ʈ�� �����ϴ� GameManager�� List�� Player ��ü�� �߰�
    public void AddPlayer(NetworkPlayer player)
    {
        players.Add(player);

        if (isServer)
            if (players.Count == NetworkManager.singleton.maxConnections)
            {
                AssignRandomOrder();
            }
    }

    [Server]
    void AssignRandomOrder()
    {
        //����
        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(0, 4);
            NetworkPlayer tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        //�÷��̾� ��ü�� ������ �Ҵ�
        for (int i = 0; i < 4; i++)
        {
            players[i].SetUp(i);
        }

        PlayerListSetUp();
    }

    [Header("Ŭ���̾�Ʈ ���� �Ϸ� �̺�Ʈ"), Space(10), Tooltip("��� �÷��̾ ������ �Ǹ� ����Ǵ� �̺�Ʈ")]
    public UnityEvent OnConnectionEvent;

    [Header("���� ���� �̺�Ʈ"), Space(10), Tooltip("������ ������ �� ����� �̺�Ʈ�� ����մϴ�.")]
    public UnityEvent OnGameStartEvent;

    [ClientRpc]
    void PlayerListSetUp()
    {
        //�� Game Manager���� List�� ������ ������
        players.Sort((a, b) => a.myOrder.CompareTo(b.myOrder));

        //ȭ�� ���̵� �� �Ŀ�, �޽��� ����Ŀ�, ���� ����
        UIController.instance.Fade.In(1.5f, () =>
        {
            UIController.instance.LineMessage.PopUp("���� ����", 3f, () =>
            {
                OnGameStartEvent?.Invoke();
            });
        });
    }
    #endregion

    /// <summary>
    /// i��° �÷��̾��� ���ʸ� ��ġ�� i + 1��° �÷��̾��� ���ʸ� �����Ѵ�.
    /// </summary>
    /// <param name="i">���� ���ʸ� ��ģ �÷��̾��� ��ȣ</param>
    [Command]
    public void NextTurn(int i)
    {
        NetworkPlayer nextPlayer = NextOrderPlayer(i);
        print($"���� ���ʸ� ��ģ �÷��̾� : {players[i]} / ���� ������ �÷��̾� : {nextPlayer}");
        // ClientRpc �Ӽ��� �ڽ��� ���ʸ� �����ϴ� �޼��� ����

        nextPlayer.CmdStartTurn();
    }

    /// <summary>
    /// ���� ������ �÷��̾��ȣ
    /// </summary>
    /// <param name="currentOrder"></param>
    public NetworkPlayer NextOrderPlayer(int currentOrder)
    {
        int nextOrder = currentOrder;
        do
        {
            nextOrder = (nextOrder + 1) % 4;
        } while (!players[nextOrder].IsGameOver);

        return players[nextOrder];
    }

    public int GetAdjacentPlayer(int currentOrder, int dir)
    {
        int adjacentPlayerNumber = currentOrder;
        do
        {
            adjacentPlayerNumber = Mathf.Abs(adjacentPlayerNumber + dir) % 4;
        } while (!players[adjacentPlayerNumber].IsGameOver);

        return adjacentPlayerNumber;
    }

    /// <summary>
    /// ���� ������ �÷��̾��� ��
    /// </summary>
    public int AliveCount
    {
        get
        {
            int aliveCount = 0;
            foreach (NetworkPlayer np in players)
            {
                if (np != null)
                    if (!np.IsGameOver)
                        aliveCount++;
            }
            return aliveCount;
        }
    }

    /// <summary>
    /// i��° �÷��̾ �� ���� ���� �ʰ��� �� ȣ��
    /// </summary>
    /// <param name="i">Ż���� �÷��̾��� ��ȣ</param>
    [Server]
    public void GameOver(int i)
    {
        if (AliveCount == 1)
        {
            NetworkPlayer winner = players.SingleOrDefault(p => p.IsGameOver == false);
            Debug.Log($"�¸� : {winner.connectionToServer}");
            ////��� �÷��̾��� ī�޶� �¸��� �÷��̾��� �ʵ�� �̵�
            FocusToWinner(winner.myOrder);
            return;
        }

        NextTurn(i);
    }

    [ClientRpc]
    void FocusToWinner(int winnerNumber)
    {
        CameraController.instance.SetVCam(winnerNumber);
    }
}
