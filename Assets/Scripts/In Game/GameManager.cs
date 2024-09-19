using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.IO;

public class GameManager : NetworkBehaviour
{

    //���� Ŭ���� ����
    public static GameManager instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            CardSetting();
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

    Commander commander;
    public static Commander Commander
    {
        get
        {
            if (instance.commander == null)
            {
                instance.commander = new Commander();
            }
            return instance.commander;
        }
    }


    [Header("ī�� �޸� �̹���"), Space(10)]
    public Sprite cardBackFace;


    [Header("�÷��̾�")]
    [SerializeField]
    List<Player> players = new List<Player>();
    Player localPlayer = null;
    public static Player LocalPlayer
    {
        get
        {
            if (instance.localPlayer == null)
                instance.localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
            return instance.localPlayer;
        }
    }
    public static Player Player(int i)
    {
        if (i < 0 || i >= 4)
        {
            Debug.Log("������ �߸��ƽ��ϴ�.");
            return null;
        }

        return instance.players[i];
    }

    [Header("ī��"), Space(10)]
    [SerializeField] Card[] cards;
    public static Card Card(int id) => instance.cards[id];
    public static int TotalCard => instance.cards.Length;

    readonly string filePath = "Conquest_Info";

    void CardSetting()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(filePath);
        string[] dataLines;

        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�.");
            return;
        }

        dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV ���Ͽ� �����Ͱ� �����մϴ�.");
            return;
        }

        int size = TotalCard;
        string line;
        string[] data;
        for (int i = 0; i < size; i++)
        {
            line = dataLines[i + 1].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            data = line.Split(',');

            //ī�� ID �ο�
            cards[i].id = int.Parse(data[0]);

            // �̹����� �ε��Ͽ� ī���� front�� �Ҵ�
            string imageName = data[1];  // ���ϸ� ����
            Sprite cardFront = Resources.Load<Sprite>("Card/" + imageName);  // Resources/Card �������� �̹��� �ε�

            if (cardFront == null)
            {
                cards[i].front = null;
                Debug.LogError($"�̹����� ã�� �� �����ϴ�: {imageName}");
            }
            else
            {
                cards[i].front = cardFront;  // ī���� front ��������Ʈ �Ҵ�
            }

            cards[i].cardName = data[2];
            
            cards[i].Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            cards[i].Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            cards[i].Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            cards[i].Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));
        }
    }

    // string ���� int�� ��ȯ�� �� enum���� ��ȯ�ϴ� �Լ�
    Attribute ParseAttribute(string value)
    {
        if (int.TryParse(value, out int result) && System.Enum.IsDefined(typeof(Attribute), result))
        {
            return (Attribute)result;
        }
        return Attribute.isEmpty;  // ��ȯ ���� �� �⺻�� ��ȯ
    }

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
            int rand = UnityEngine.Random.Range(i, 4);
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
            .Clear()
            .Add_While(() => UIMaster.Fade.In(1.5f), UIMaster.Fade.IsPlaying)
            .WaitSeconds(1f)
            .Add_While(() => UIMaster.Message.PopUp("���� ����", 3f), UIMaster.Message.IsPlaying)
            .Add(() => OnGameStartEvent?.Invoke())
            .Play();
    }
    #endregion



    public int CurrentOrder { get; private set; }



    /// <summary>
    /// true : ������������ ������ ����˴ϴ�.
    /// false : ������������ ������ ����˴ϴ�.
    /// </summary>
    public bool isClockwise = true;

    /// <summary>
    /// ���� ���� ������ ��ȯ
    /// </summary>
    public static int NextOrder(int myOrder)
    {
        int nextOrder = myOrder;

        do
        {
            nextOrder = (nextOrder + (instance.isClockwise ? 1 : 3)) % 4;
        }
        while (Player(nextOrder).isGameOver);

        return nextOrder;
    }

    /// <summary>
    /// ���� ���� ������ ��ȯ
    /// </summary>
    public static int PreviousOrder(int myOrder)
    {
        int nextOrder = myOrder;

        do
        {
            nextOrder = (nextOrder + (instance.isClockwise ? 3 : 5)) % 4;
        }
        while (Player(nextOrder).isGameOver);

        return nextOrder;
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

            if (isFinished)
            {
                foreach (Player np in instance.players)
                {
                    if (np != null)
                        np.hasTurn = false;
                }
            }

            return isFinished;
        }
    }


    public static Dictionary<int, List<Card>> dict_HandLimitStack = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };

    /// <summary>
    /// �ڽ��� �� ���� �÷��̾ �ߵ��� ī���� �� ���� ȿ���� ���� ����Ͽ� �ڽ��� �� ���� ���� �����ɴϴ�.
    /// </summary>
    public static int GetPlayerHandLimit(int i)
    {
        int @default = 6;
        int previousOrder = PreviousOrder(i);
        foreach (Card c in dict_HandLimitStack[previousOrder])
        {
            @default -= 1;
            //@default += c.Limit...;
        }
        return @default;
    }

    public static Dictionary<int, List<Card>> dict_OnDrawEvent = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };

    public static Dictionary<int, List<Card>> dict_OnDeployedEvent = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };
}