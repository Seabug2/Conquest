using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class GameManager : NetworkBehaviour
{
    Commander commander;
    public static Commander Commander
    {
        get
        {
            if (instance.commander == null)
            {
                instance.commander = instance.gameObject.AddComponent<Commander>();
            }
            return instance.commander;
        }
    }

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
    [Header("�÷��̾� ����Ʈ"), Space(10)]
    List<Player> players = new List<Player>();

    Player localPlayer = null;
    public Player LocalPlayer
    {
        get
        {
            if (instance.localPlayer == null)
                instance.localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
            return instance.localPlayer;
        }
    }

    /// <returns>i��° ������ �÷��̾�</returns>
    public static Player Player(int i) => instance.players[i];

    [Header("����"), Space(10)]
    [SerializeField] Card[] cards;
    public static Card Card(int id) => instance.cards[id];

    /// <summary>
    /// ������ ��� ī�� ��� (���� ��)
    /// </summary>
    public static int TotalCard => instance.cards.Length;

    [Header("��")]
    [SerializeField] Deck deck;
    public static Deck Deck => instance.deck;

    [Header("�ʵ�")]
    [SerializeField] Field[] fields;
    public static Field Field(int i) =>
        GameManager.instance.fields[i];

    [Header("��")]
    [SerializeField] Hand[] hands;
    public static Hand Hand(int i) => instance.hands[i];

    #region #1 �÷��̾��� ����� ���� ����
    //�� Ŭ���̾�Ʈ�� �����ϴ� GameManager�� List�� Player ��ü�� �߰�
    public void AddPlayer(Player player)
    {
        players.Add(player);

        if (players.Count == 4)
        {
            ServerGameStart();
        }
    }

    [Header("Ŭ���̾�Ʈ ���� �Ϸ� �̺�Ʈ"), Space(10), Tooltip("��� �÷��̾ ������ �Ǹ� ����Ǵ� �̺�Ʈ")]
    public UnityEvent OnConnectionEvent;

    [Header("���� ���� �̺�Ʈ"), Space(10), Tooltip("������ ������ �� ����� �̺�Ʈ�� ����մϴ�.")]
    public UnityEvent OnGameStartEvent;

    [ServerCallback]
    void ServerGameStart()
    {
        ShufflePlayers();
        RpcGameStart();
    }

    [Server]
    void ShufflePlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(i, 4);
            Player tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        // �÷��̾� ��ü�� ������ �Ҵ�
        for (int i = 0; i < 4; i++)
        {
            players[i].order = i;
        }
    }

    [ClientRpc]
    void RpcGameStart()
    {
        //�� Game Manager���� List�� ������ ������
        players.Sort((a, b) => a.order.CompareTo(b.order));

        CurrentOrder = 0;

        Commander
            .Add(() => UIMaster.Fade.In(1.5f), 1.5f)
            .Add(1f, () => UIMaster.LineMessage.PopUp("���� ����", 3f), 3f)
            .Add(() => OnGameStartEvent?.Invoke(), 3f)
            .Play(false);

        ////ȭ�� ���̵� �� �Ŀ�, �޽��� ����Ŀ�, ���� ����
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
        while (players[CurrentOrder].isGameOver || players[CurrentOrder].isTurnSkipped);

        return CurrentOrder;
    }

    /// <summary>
    /// i��° �÷��̾��� ���� ������ �÷��̾��ȣ
    /// </summary>
    /// <param name="currentOrder"></param>
    public Player NextPlayer(int currentOrder)
    {
        int nextOrder = currentOrder;
        do
        {
            nextOrder = (nextOrder + 1) % 4;
        } while (!players[nextOrder].isGameOver);

        return players[nextOrder];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Player NextPlayer()
    {
        int nextOrder = CurrentOrder;
        do
        {
            nextOrder = (nextOrder + 1) % 4;
        } while (!players[nextOrder].isGameOver);

        return players[nextOrder];
    }

    public int GetAdjacentPlayer(int currentOrder, int dir)
    {
        int adjacentPlayerNumber = currentOrder;
        do
        {
            adjacentPlayerNumber = Mathf.Abs(adjacentPlayerNumber + dir) % 4;
        } while (!players[adjacentPlayerNumber].isGameOver);

        return adjacentPlayerNumber;
    }

    /// <summary>
    /// ���� ������ �÷��̾��� ��
    /// </summary>
    public static int AliveCount
    {
        get
        {
            int aliveCount = 0;
            foreach (Player np in instance.players)
            {
                if (np != null)
                    if (!np.isGameOver)
                        aliveCount++;
            }
            return aliveCount;
        }
    }

    /// <summary>
    /// ��� �÷��̾ �ڽ��� ���ʸ� �������� �� ���尡 ���� ����.
    /// </summary>
    public static bool RoundFinished
    {
        get
        {
            bool isFinished = true;
            foreach (Player np in instance.players)
            {
                if (np != null)
                    if (!np.hasTurn)
                    {
                        isFinished = false;
                        break;
                    }
            }
            return isFinished;
        }
    }
}