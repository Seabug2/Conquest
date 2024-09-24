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

    #region �ʱ�ȭ
    string filePath;
    DeckIDList deckIdList;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "DeckIdList.json");

        // ���� �ε�
        deckIdList = LoadDeckIdList();

        CardSetting();
    }
    #endregion

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
                        deck.ServerDraftPhase();
                        return;
                    }

                    StartTurn();
                }
            }
            else
            {
                RpcEndGame();
            }
        }
    }

    /// <summary>
    /// Server
    /// </summary>
    [Server]
    void PlayerShuffle()
    {
        int count = players.Count;
        int rand;
        Player tmp;
        for (int i = 0; i < count; i++)
        {
            rand = UnityEngine.Random.Range(i, count); // i���� ���� �ε������� ����
            tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        for (int i = 0; i < count; i++)
        {
            players[i].order = i;
        }
    }

    [Command(requiresAuthority = false)]
    public void Ackn_SortPlayerList(int reply)
    {
        if (reply < 0 || reply >= isAcknowledged.Length) return;
        isAcknowledged[reply] = true;

        if (IsAllReceived)
        {
            RpcSortPlayerList();
        }
    }

    [ClientRpc]
    void RpcSortPlayerList()
    {
        players.Sort((a, b) => a.order.CompareTo(b.order));
        Ackn_StartGame(LocalPlayer.order);
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
    public static int TotalCard => instance.cards.Length;

    void CardSetting()
    {
        if (cards.Length == 0) return;

        TextAsset csvFile = Resources.Load<TextAsset>("Conquest_Info");

        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�.");
            return;
        }

        string[] dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV ���Ͽ� �����Ͱ� �����մϴ�.");
            return;
        }

        //ī�忡 ȿ���� �Ҵ��ϱ� ���� �Ŵ���
        AbilityManager abilityManager = new AbilityManager();

        for (int i = 0; i < 54; i++)
        {
            cards[i].id = deckIdList[i];
            dict_Card.Add(deckIdList[i], cards[i]);

            int lineIndex = cards[i].id + 1;
            if (lineIndex >= dataLines.Length)
            {
                Debug.LogError($"������ ���� �ε����� ������ ������ϴ�: {lineIndex}");
                continue;
            }

            string line = dataLines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                Debug.LogWarning($"�� ���� �߰ߵǾ����ϴ�: {lineIndex}");
                continue;
            }



            string[] data = line.Split(',');
            //if (data.Length < �ּ�����_������_����)
            //{
            //    Debug.LogError($"�����Ͱ� �����մϴ�: {lineIndex}");
            //    continue;
            //}



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

            cards[i].name = data[2];
            cards[i].cardName = data[2];

            cards[i].Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            cards[i].Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            cards[i].Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            cards[i].Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));


            // TODO ī�� ȿ�� ������ �Ϸ��ϸ� ����
            continue;

            if (int.TryParse(data[11], out int abilityId))
            {
                string[] extractedData = data.Skip(12).Take(4).ToArray();
                cards[i].ability = abilityManager.Create(abilityId, extractedData);
            }
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

    #region ����
    /// <summary>
    /// Rpc�� ���� ���� �÷��̾ ���ʸ� �����ϸ� currentOrder�� ���ŵȴ�.
    /// </summary>
    public int currentOrder = 0;

    public int firstOrder = 0;

    public int FirstOrder()
    {
        currentOrder = 0;

        while (players[currentOrder] == null || players[currentOrder].isGameOver)
        {
            currentOrder = (currentOrder + (IsClockwise ? 1 : maxPlayer - 1)) % maxPlayer;
        }

        return currentOrder;
    }

    public int NextOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (IsClockwise ? 1 : maxPlayer - 1)) % maxPlayer;
        }
        while (players[_Order] == null || players[_Order].isGameOver);

        return _Order;
    }

    public int PreviousOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (IsClockwise ? maxPlayer - 1 : 1)) % maxPlayer;
        }
        while (players[_Order] == null || players[_Order].isGameOver);

        return _Order;
    }

    /// <summary>
    /// true : �������� 0 => 1
    /// false : �������� 0 <= 1
    /// </summary>
    public bool IsClockwise { get; private set; }

    #endregion

    public readonly bool[] isAcknowledged = new bool[4];

    public bool IsAllReceived
    {
        get
        {
            foreach (bool b in isAcknowledged)
            {
                if (!b) return false;
            }

            for (int i = 0; i < isAcknowledged.Length; i++)
            {
                isAcknowledged[i] = false;
            }

            return true;
        }
    }

    public static Deck deck;

    public static Dictionary<int, Field> dict_Field = new();

    public static Dictionary<int, Hand> dict_Hand = new();

    #region ����
    public bool IsPlaying { get; private set; }

    public static bool RoundFinished
    {
        get
        {
            foreach (Player p in instance.players)
            {
                if (p == null) continue;

                //���ʸ� ������ ���� �÷��̾ �ִٸ�...
                if (!p.hasTurn)
                {
                    return false;
                }
            }

            foreach (Player p in instance.players)
            {
                if (p != null)
                    p.hasTurn = false;
            }

            return true;
        }
    }

    [Space(20)]
    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnStartEvent;

    [Command(requiresAuthority = false)]
    void Ackn_StartGame(int reply)
    {
        if (reply < 0 || reply >= isAcknowledged.Length) return;
        isAcknowledged[reply] = true;

        if (IsAllReceived)
        {
            RpcStartGame();
        }
    }

    /// <summary>
    /// Server
    /// </summary>
    [Server]
    public void StartGame()
    {
        RpcStartGame();
    }

    [ClientRpc]
    public void RpcStartGame()
    {
        IsPlaying = true;
        Commander commander = new();
        commander
            .Add(CameraController.instance.FocusOnCenter)
            .Add(() => UIManager.Fade.In(3f))
            .WaitSeconds(3.3f)
            .Add(() => UIManager.Message.ForcePopUp("������ 1���� �Ǽ���!", 5f), 5.5f)
            .Add(deck.ServerDraftPhase)
            .Play();
    }

    [Server]
    void DraftPhase()
    {
        deck.ServerDraftPhase();
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
            RpcEndGame();
        }
    }

    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnEndGame;

    [ClientRpc]
    void RpcEndGame()
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


    #region Json : �� ����Ʈ
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
    #endregion
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