using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //���� Ŭ���� ����
    public static GameManager instance = null;
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

    NetworkPlayer localPlayer = null;
    public static NetworkPlayer LocalPlayer
    {
        get
        {
            if (instance.localPlayer == null)
                instance.localPlayer = NetworkClient.localPlayer.GetComponent<NetworkPlayer>();
            return instance.localPlayer;
        }
    }
    public static NetworkPlayer Player(int i) => instance.players[i];

    [Header("����"), Space(10)]
    [SerializeField] Card[] cards;
    public static Card Card(int id) => instance.cards[id];
    /// <summary>
    /// ������ ��� ī�� ��� (���� ��)
    /// </summary>
    public static int TotalCard => instance.cards.Length;

    [Header("��")]
    [SerializeField] Deck deck;
    public Deck Deck => deck;

    [Header("�ʵ�")]
    [SerializeField] Field[] fields;
    public static Field Field(int i) =>
        GameManager.instance.fields[i];

    [Header("��")]
    [SerializeField] Hand[] hands;
    public static Hand Hand(int i) => instance.hands[i];

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
            int rand = Random.Range(i, 4);
            NetworkPlayer tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        //�÷��̾� ��ü�� ������ �Ҵ�
        for (int i = 0; i < 4; i++)
        {
            players[i].SetUp(i);
        }

        players.Sort((a, b) => a.myOrder.CompareTo(b.myOrder));

        GameStart();
    }

    [Header("Ŭ���̾�Ʈ ���� �Ϸ� �̺�Ʈ"), Space(10), Tooltip("��� �÷��̾ ������ �Ǹ� ����Ǵ� �̺�Ʈ")]
    public UnityEvent OnConnectionEvent;

    [Header("���� ���� �̺�Ʈ"), Space(10), Tooltip("������ ������ �� ����� �̺�Ʈ�� ����մϴ�.")]
    public UnityEvent OnGameStartEvent;

    [ClientRpc]
    void GameStart()
    {
        //�� Game Manager���� List�� ������ ������
        CurrentOrder = 0;
        //ȭ�� ���̵� �� �Ŀ�, �޽��� ����Ŀ�, ���� ����
        UIMaster.Fade.In(1.5f, () =>
        {
            UIMaster.LineMessage.PopUp("���� ����", 3f, () =>
            {
                OnGameStartEvent?.Invoke();
            });
        });
    }
    #endregion

    public int CurrentOrder { get; private set; }

    /// <summary>
    /// ������ �ѱ� ���� (true : �������� / false : ��������)
    /// </summary>
    public bool isClockwise = true;

    /// <param name="step">���� ���� �������� ���ʸ� �ѱ�ϴ�.</param>
    /// <returns>������ ���� ��ŭ ������ �ѱ� �� �ֽ��ϴ�.</returns>
    int NextOrder(int step = 1)
    {
        if (step < 1)
        {
            step = 1;
        }
        do
        {
            CurrentOrder = (CurrentOrder + (isClockwise ? step : -step) + 4) % 4;
        }
        while (players[CurrentOrder].IsGameOver || players[CurrentOrder].IsTurnSkipped);

        return CurrentOrder;
    }

    [Command]
    public void NextTurn()
    {
        NetworkPlayer nextPlayer = NextOrderPlayer(CurrentOrder);
        print($"���� ���ʸ� ��ģ �÷��̾� : {players[CurrentOrder]} / ���� ������ �÷��̾� : {nextPlayer}");
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
}
