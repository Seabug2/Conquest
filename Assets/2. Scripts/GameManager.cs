using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.IO;

[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    #region �̱���
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            IsPlaying = false;
            //CardSetting();


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
    #endregion

    string filePath;
    DeckIDList deckIdList;
    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "DeckIdList.json");

        // ���� �ε�
        deckIdList = LoadDeckIdList();

        CardSetting();
    }

    public bool IsPlaying { get; private set; }

    #region �÷��̾�
    public const int maxPlayer = 4;
    [SerializeField, Header("�÷��̾� ����Ʈ")]
    List<Player> players = new();
    public static Player GetPlayer(int i)
    {
        if (i < 0 || i >= maxPlayer)
        {
            Debug.LogError("�߸��� �ε���");
            return null;
        }
        return instance.players[i];
    }
    public Player LocalPlayer { get; private set; }

    public void AddPlayer(Player _player)
    {
        if (_player.isLocalPlayer)
        {
            LocalPlayer = _player;
        }

        players.Add(_player);

        if (isServer && players.Count == maxPlayer)
        {
            PlayerShuffle();
        }
    }

    public void RemovePlayer(Player _player)
    {
        if (players.Contains(_player))
        {
            //������ ���� �������� ���� ���¶��,
            if (!IsPlaying)
            {
                //�÷��̾� ����Ʈ���� �����Ͽ� ����Ʈ�� ũ�⸦ ������� �Ѵ�.
                players.Remove(_player);
                return;
            }

            int i = players.IndexOf(_player);
            players[i] = null;
        }

        if (isServer)
        {
            if (AliveCount > 1)
            {

                if (_player.isMyTurn)
                {
                    if (RoundFinished)
                    {
                        DraftPhase();
                        return;
                    }

                    StartTurn();
                }
            }
            else
            {
                EndGame();
            }
        }
    }


    [Server]
    void PlayerShuffle()
    {
        int[] order = new int[maxPlayer];
        for (int i = 0; i < maxPlayer; i++)
        {
            order[i] = i;
        }
        for (int i = 0; i < maxPlayer; i++)
        {
            int rand = UnityEngine.Random.Range(i, maxPlayer); // i���� ���� �ε������� ����
            int tmp = order[i];
            order[i] = order[rand];
            order[rand] = tmp;
        }

        RpcPlayerShuffle(order);
    }

    [ClientRpc]
    void RpcPlayerShuffle(int[] order)
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            players[i].order = order[i];
        }
        players.Sort((a, b) => a.order.CompareTo(b.order));
    }

    public static int AliveCount
    {
        get
        {
            int alivePlayerCount = maxPlayer;

            foreach (Player p in instance.players)
            {
                //Ŭ���̾�Ʈ�� �����߰ų�,
                //���� ������ �÷��̾�
                if (p == null || p.isGameOver)
                {
                    alivePlayerCount--;
                }
            }

            return alivePlayerCount;
        }
    }
    #endregion

    #region ī��
    [Header("ī�� �޸� �̹���"), Space(10)]
    public Sprite cardBackFace;

    [Header("ī��"), Space(10)]
    [SerializeField] Card[] cards;
    readonly Dictionary<int, Card> dict_Card = new();
    public static Card Card(int id) => instance.dict_Card[id];
    public static int TotalCard => instance.dict_Card.Count;

    //readonly string filePath = "Conquest_Info";

    void CardSetting()
    {
        if (cards.Length == 0) return;

        //54���� ī�� �⺻���� �����ϰ�...
        for (int i = 0; i < 54; i++)
        {
            cards[i].id = deckIdList[i];
            dict_Card.Add(deckIdList[i], cards[i]);
        }

        TextAsset csvFile = Resources.Load<TextAsset>("Conquest_Info");
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

        //int size = TotalCard;
        //string line;
        //string[] data;

        foreach (Card c in dict_Card.Values)
        {
            string line = dataLines[c.id + 1].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] data = line.Split(',');

            string imageName = data[1];  // ���ϸ� ����
            Sprite cardFront = Resources.Load<Sprite>("Card/" + imageName);  // Resources/Card �������� �̹��� �ε�

            if (cardFront == null)
            {
                c.front = null;
                Debug.LogError($"�̹����� ã�� �� �����ϴ�: {imageName}");
            }
            else
            {
                c.front = cardFront;  // ī���� front ��������Ʈ �Ҵ�
            }

            c.name = data[2];
            c.cardName = data[2];

            c.Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            c.Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            c.Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            c.Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));

            /*
            Type abilityType = Type.GetType(data[11]);

            IAbility ability = (IAbility)Activator.CreateInstance(abilityType);
            string[] extractedData = data.Skip(12).Take(4).ToArray();
            ability.SetValue(extractedData);
            c.ability = ability;
            */
        }
    }
    Attribute ParseAttribute(string value)
    {
        if (int.TryParse(value, out int result) && System.Enum.IsDefined(typeof(Attribute), result))
        {
            return (Attribute)result;
        }
        return Attribute.isEmpty;  // ��ȯ ���� �� �⺻�� ��ȯ
    }
    #endregion

    public static Deck deck;

    public static Dictionary<int, Field> dict_Field = new();

    public static Dictionary<int, Hand> dict_Hand = new();

    #region ����
    /// <summary>
    /// Rpc�� ���� ���� �÷��̾ ���ʸ� �����ϸ� currentOrder�� ���ŵȴ�.
    /// </summary>
    public int currentOrder = 0;
    /// <summary>
    /// true : �������� 0 => 1
    /// false : �������� 0 <= 1
    /// </summary>
    public bool IsClockwise { get; private set; }

    [Space(20)]
    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnStartEvent;

    [ClientRpc]
    public void RpcGameStart()
    {
        IsPlaying = true;
    }

    [Server]
    void DraftPhase()
    {

    }

    [Server]
    void StartTurn()
    {
        GetPlayer(NextOrder(currentOrder)).isMyTurn = true;
        GetPlayer(currentOrder).hasTurn = true;
        GetPlayer(NextOrder(currentOrder)).RpcStartTurn();
    }

    [Server]
    public void EndTurn()
    {
        GetPlayer(currentOrder).isMyTurn = false;

        if (GetPlayer(currentOrder).Hand.IsLimitOver)
        {
            GetPlayer(currentOrder).isGameOver = true;
        }

        if (AliveCount > 1)
        {
            if (RoundFinished)
            {
                //�巡��Ʈ...
                return;
            }
            StartTurn();
        }
        else
        {
            EndGame();
        }
    }

    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnEndGame;

    [ClientRpc]
    void EndGame()
    {
        OnEndGame?.Invoke();
        IsPlaying = false;

        if (LocalPlayer.isGameOver)
        {
            //�й� �̺�Ʈ
        }
        else
        {
            //�¸� �̺�Ʈ
        }
    }
    #endregion

    /// <summary>
    /// ���� ���� ������ ��ȯ
    /// </summary>
    public int NextOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (IsClockwise ? 1 : 3)) % maxPlayer;
        }
        while (players[_Order] == null || players[_Order].isGameOver);

        return _Order;
    }

    /// <summary>
    /// ���� ���� ������ ��ȯ
    /// </summary>
    public static int PreviousOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (instance.IsClockwise ? 3 : 5)) % 4;
        }
        while (GetPlayer(_Order) == null || GetPlayer(_Order).isGameOver);

        return _Order;
    }

    /// <summary>
    /// ��� �÷��̾ �ڽ��� ���ʸ� �������� �� ���尡 ���� ����.
    /// </summary>
    public static bool RoundFinished
    {
        get
        {
            foreach (Player np in instance.players)
            {
                if (np == null) continue;

                //���ʸ� ������ ���� �÷��̾ �ִٸ�...
                if (!np.hasTurn)
                {
                    return false;
                }
            }

            foreach (Player np in instance.players)
            {
                if (np != null)
                    np.hasTurn = false;
            }

            return true;
        }
    }

    /*
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
    */

    public DeckIDList LoadDeckIdList()
    {
        if (File.Exists(filePath))
        {
            // ������ �����ϸ� ���Ͽ��� JSON�� �а� ��ü�� ��ȯ
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DeckIDList>(json);
        }
        else
        {
            // ������ ������ �⺻ DeckIDList ��ü�� ����
            DeckIDList newDeck = new DeckIDList();
            SaveDeckIdList(newDeck);  // �⺻ DeckIDList�� ���Ϸ� ����
            return newDeck;
        }
    }

    // ���� ����
    public void SaveDeckIdList(DeckIDList deckIdList)
    {
        // JSON ���ڿ��� ��ȯ
        string json = JsonUtility.ToJson(deckIdList, true);

        // ���Ϸ� ����
        File.WriteAllText(filePath, json);

        Debug.Log($"DeckIdList �����: {filePath}");
    }
}

[Serializable]
public class DeckIDList
{
    public int[] idList;

    public int this[int i]
    {
        get
        {
            if (i < 0 || i >= idList.Length)
            {
                Debug.LogError("���� ����");
                return 0;
            }
            return idList[i];
        }
    }

    public DeckIDList()
    {
        idList = new int[54];

        for (int i = 0; i < 54; i++)
        {
            idList[i] = i;
        }
    }

    public DeckIDList(int[] idList)
    {
        this.idList = idList;
    }
}