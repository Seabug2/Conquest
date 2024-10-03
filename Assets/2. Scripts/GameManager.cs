using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum GamePhase
{
    Standby,
    DraftPhase,
    PlayerPhase,
    EndPhase
}


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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        commander.Cancel();

        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    [Header("��, �ʵ�, �� ����")]
    [SerializeField] Deck deck;
    public static Deck Deck => instance.deck;
    [SerializeField] private Field[] fields;
    public static readonly Dictionary<int, Field> dict_Field = new();
    [SerializeField] private Hand[] hands;
    public static readonly Dictionary<int, Hand> dict_Hand = new();

    [HideInInspector]
    public GamePhase CurrentPhase = GamePhase.Standby;

    public int[] deckIds;

    readonly Commander commander = new();

    [Space(20)]
    [Header("�÷��̾� ���� �̺�Ʈ")]
    public UnityEvent ConnectionEvent = new();

    //[Space(20)]
    //[Header("���� �Ϸ� �̺�Ʈ")]
    //public UnityEvent CompleteConnect = new ();

    [Space(20)]
    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnStartEvent = new();

    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnEndGame = new();


    #region �ʱ�ȭ
    void Start()
    {
        InitializeFieldAndHandDictionaries();
    }

    private void InitializeFieldAndHandDictionaries()
    {
        foreach (Field f in fields)
        {
            dict_Field.Add(f.SeatNum, f);
            f.TileSet();
        }

        foreach (Hand h in hands)
        {
            dict_Hand.Add(h.SeatNum, h);
        }
    }

    public override void OnStartServer()
    {
        SetUpDeck();
    }

    [Server]
    void SetUpDeck()
    {
        deckIds = LoadDeckIdList().idList;
        deck.SetUpDeck(deckIds);
    }

    #endregion

    #region �ʱ� ����
    //�÷��̾� 4���� ������ ���
    public void AddPlayer(Player _player)
    {
        if (!CurrentPhase.Equals(GamePhase.Standby)) return;

        if (_player.isLocalPlayer)
        {
            LocalPlayer = _player;
        }

        players.Add(_player);
        ConnectionEvent.Invoke();

        if (players.Count == maxPlayer)
        {
            StartGame();
        }
    }

    [ServerCallback]
    async void StartGame()
    {
        PlayerShuffle();

        await WaitForAllAcknowledgements();

        //ȣ��Ʈ�� �ƴ� ������ ��쿡�� ����ȭ �� �� �ֵ���...
        if (isServerOnly)
        {
            players.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        RpcSortPlayerList();

        await WaitForAllAcknowledgements();

        SyncDeckIdList(deckIds);

        await WaitForAllAcknowledgements();

        //TODO �� ID ����Ʈ�� ����
        ServerSpawnCard();

        await WaitForAllAcknowledgements();

        RpcStartGame();

        await WaitForAllAcknowledgements();

        deck.ServerStartDraftPhase();
    }

    [Server]
    public void PlayerShuffle()
    {
        int rand;
        Player tmp;
        for (int i = 0; i < maxPlayer; i++)
        {
            rand = UnityEngine.Random.Range(i, maxPlayer); // i���� ���� �ε������� ����
            tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        for (int i = 0; i < maxPlayer; i++)
        {
            //ȣ��Ʈ�� �ƴ� ���������� ������ ����ȭ �� �� �ֵ���...
            if (isServerOnly)
            {
                players[i].SetOrder(i);
            }

            players[i].RpcSetOrder(i);
        }
    }

    [ClientRpc]
    void RpcSortPlayerList()
    {
        players.Sort((a, b) => a.Order.CompareTo(b.Order));
        UISetting();
        CmdReply(LocalPlayer.Order);
    }

    void UISetting()
    {
        UIManager.GetUI<YourNumber>().Init(LocalPlayer.Order);
        UIManager.GetUI<SideMenu>().Initialize();
        HeadLine hl = UIManager.GetUI<HeadLine>();
        hl.localPlayerOrder = LocalPlayer.Order;
        TurnChangeEvent += hl.IsMyTurn;
    }

    [ClientRpc]
    void SyncDeckIdList(int[] deckIds)
    {
        this.deckIds = deckIds;
        CmdReply(LocalPlayer.Order);
    }

    [Server]
    void ServerSpawnCard()
    {
        if (deckIds.Length == 0)
        {
            Debug.LogError("���� : �� ID ����Ʈ�� �ҷ����µ� ������ �� �����ϴ�.");
            return;
        }

        NetworkManager manager = NetworkManager.singleton;

        //�������� ī�带 ����
        foreach (int i in deckIds)
        {
            GameObject card = Instantiate(manager.spawnPrefabs[0]
                , deck.transform.position, Quaternion.identity);
            card.GetComponent<Card>().id = deckIds[i];
            NetworkServer.Spawn(card);
        }
    }

    /// <summary>
    /// <see cref="Card.Register"/>
    /// </summary>
    public void RegisterCard(int id, Card card)
    {
        if (id >= 0 && !dict_Card.ContainsKey(id))
        {
            dict_Card.Add(id, card);

            if (dict_Card.Count == deckIds.Length)
            {
                Debug.Log(dict_Card.Count);

                //id�� �����Ͽ� ī�带 ����...
                CardSetting();
            }
        }
    }

    void CardSetting()
    {
        int length = dict_Card.Count;
        if (length == 0) return;

        string[] dataLines = LoadCSVData();
        AbilityManager abilityManager = new();

        for (int i = 0; i < length; i++)
        {
            Card card = dict_Card[deckIds[i]];
            SetCardData(card, dataLines, abilityManager);
        }

        CmdReply(LocalPlayer.Order);
    }

    [ClientRpc]
    public void RpcStartGame()
    {
        commander
            .Refresh()
            .Add(() =>
            {
                CameraController.instance.FocusOnCenter();
                UIManager.GetUI<Fade>().In(3f);
            }
            , 3.3f)
            .Add(() =>
            {
                UIManager.GetUI<Timer>().Active(1.8f);
                UIManager.GetUI<HeadLine>().On(1.8f);
            }
            , 2f)
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("������ 1���� �Ǽ���!", 3f))
            .Add(() => CmdReply(LocalPlayer.Order))
            .Play();
    }
    #endregion

    #region ����
    readonly bool[] isAcknowledged = new bool[maxPlayer];

    [Command(requiresAuthority = false)]
    public void CmdReply(int order)
    {
        Debug.Log($"{order} ���� ��");

        if (order < 0 || order >= maxPlayer)
        {
            Debug.LogError($"�ε��� ���� ({order}) : ���� �ʰ�");
            return;
        }

        isAcknowledged[order] = true;
        Debug.Log($"{order} : {isAcknowledged[order]}");
    }

    public bool IsAllReceived()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            Player player = GetPlayer(i);
            if (player == null || player.isGameOver) continue;
            if (!isAcknowledged[i]) return false;
        }

        return true;
    }

    [Server]
    public void ResetAcknowledgements()
    {
        Array.Clear(isAcknowledged, 0, isAcknowledged.Length);
    }

    [Server]
    public async UniTask WaitForAllAcknowledgements(int timeoutMilliseconds = 20000)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(timeoutMilliseconds);

        try
        {
            await UniTask.WaitUntil(IsAllReceived, cancellationToken: cts.Token);
            Debug.Log("���� �Ϸ�!");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("������ ���� ���� �÷��̾ �ֽ��ϴ�. ���� ���¸� Ȯ���մϴ�.");
            CheckDisconnectedPlayers();
        }

        ResetAcknowledgements();
    }

    [Server]
    public void CheckDisconnectedPlayers()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            if (isAcknowledged[i]) continue;

            Player player = GetPlayer(i);
            if (player == null)
            {
                Debug.LogError($"Player {i}�� ������ �����߽��ϴ�...");
            }
            else if (player.isGameOver)
            {
                Debug.LogError($"Player {i}�� Ż�������Ƿ� �׳� �Ѿ�ϴ�.");
            }
            else
            {
                Debug.LogError($"Player {i}�� ������ �����ϴ�...");
                // TODO: �������� �ʴ� �÷��̾ ���� ���� ó�� �ʿ�
            }
        }
    }
    #endregion

    #region �÷��̾�
    public const int maxPlayer = 4;

    //[SerializeField, Header("�÷��̾� ����Ʈ")]
    readonly
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

    public static Player LocalPlayer { get; private set; }

    [Server]
    public void RemovePlayer(Player _player)
    {
        //�÷��̾� ����Ʈ�� �߰��� �÷��̾��� ���
        if (players.Contains(_player))
        {
            //������ ���� �������� ���� ���¶��,
            if (CurrentPhase.Equals(GamePhase.Standby))
            {
                //�÷��̾� ����Ʈ���� �����Ͽ� ����Ʈ�� ũ�⸦ ������� �Ѵ�.
                players.Remove(_player);
            }
            else
            {
                int i = players.IndexOf(_player);
                players[i] = null;
            }

            if (AliveCount > 1)
            {
                if (_player.isMyTurn)
                {
                    EndTurn(_player.Order);
                    return;
                }
                else
                {
                    isAcknowledged[_player.Order] = true;
                }
            }
            else
            {
                //� �÷��̾ ������ �������� ��
                //���� ������ �� ���̶�� �� ��� ������ ���� ����.
                RpcEndGame(LastPlayer());
                return;
            }
        }

        //�÷��̾� ����Ʈ�� �߰��� �÷��̾� ��ü�� �ƴϾ��� ��� ó�� ����
    }

    [Server]
    int LastPlayer()
    {
        return players.Where(p => p != null && !p.isGameOver)
                      .Select(p => p.Order)
                      .LastOrDefault();
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
    [SerializeField] Card prefab;

    /// <summary>
    /// �̸� �����ص� ���¸� �����Ͽ� ��Ȱ�뼺�� ���Դϴ�.
    /// </summary>
    public readonly ICardState noneState = new NoneState();

    readonly Dictionary<int, Card> dict_Card = new();

    public static Card Card(int id)
    {
        if (!instance.dict_Card.ContainsKey(id))
        {
            Debug.LogError("�������� �ʴ� ī���Դϴ�");
            return null;
        }
        return instance.dict_Card[id];
    }
    public static int TotalCard => instance.dict_Card.Count;

    private string[] LoadCSVData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Conquest_Info");

        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�.");
            return Array.Empty<string>();
        }

        string[] dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV ���Ͽ� �����Ͱ� �����մϴ�.");
            return Array.Empty<string>();
        }

        return dataLines;
    }
    private void SetCardData(Card card, string[] dataLines, AbilityManager abilityManager)
    {
        card.iCardState = noneState;
        SetCardEventHandlers(card);
        ParseCardData(card, dataLines, abilityManager);
    }

    private void SetCardEventHandlers(Card card)
    {
        if (CameraController.instance != null)
        {
            card.OnPointerCardDown = (c) => CameraController.instance.Freeze(true);
            card.OnPointerCardUp = (c) => CameraController.instance.Freeze(false);
        }

        card.OnPointerCardEnter = (card) => UIManager.GetUI<Info>()?.PopUp(card);
    }

    private void ParseCardData(Card card, string[] dataLines, AbilityManager abilityManager)
    {
        int lineIndex = card.id + 1;
        if (lineIndex >= dataLines.Length)
        {
            Debug.LogError($"������ ���� �ε����� ������ ������ϴ�: {lineIndex}");
            return;
        }

        string line = dataLines[lineIndex].Trim();
        if (string.IsNullOrWhiteSpace(line)) return;

        string[] data = line.Split(',');

        LoadCardImage(card, data[1]);
        card.name = data[2];
        card.cardName = data[2];
        SetCardAttributes(card, data);
    }

    private void LoadCardImage(Card card, string imageName)
    {
        Sprite cardFront = Resources.Load<Sprite>("Card/" + imageName);

        if (cardFront == null)
        {
            Debug.LogError($"�̹����� ã�� �� �����ϴ�: {imageName}");
        }
        else
        {
            card.SetSprite(cardFront, cardBackFace);
        }
    }

    private void SetCardAttributes(Card card, string[] data)
    {
        for (int i = 0; i < 4; i++)
        {
            int attributeIndex = 3 + i * 2;
            card.Sockets[i] = new Socket(ParseAttribute(data[attributeIndex]), data[attributeIndex + 1].Equals("1"));
        }
    }

    Attribute ParseAttribute(string value)
    {
        if (int.TryParse(value, out int result) && System.Enum.IsDefined(typeof(Attribute), result))
        {
            return (Attribute)result;
        }
        return Attribute.IsEmpty;  // ��ȯ ���� �� �⺻�� ��ȯ
    }
    #endregion

    #region ����
    int currentOrder;

    public static int CurrentOrder => instance.currentOrder;

    [Server]
    public void SetCurrentOrder(int newOrder)
    {
        if (isServer)
        {
            GetPlayer(currentOrder).isMyTurn = false;

            currentOrder = newOrder;

            GetPlayer(currentOrder).isMyTurn = true;
        }

        RpcSetCurrentOrder(newOrder);
    }

    public event Action<int> TurnChangeEvent;

    [ClientRpc]
    public void RpcSetCurrentOrder(int newOrder)
    {
        GetPlayer(currentOrder).isMyTurn = false;

        currentOrder = newOrder;

        GetPlayer(currentOrder).isMyTurn = true;

        TurnChangeEvent?.Invoke(newOrder);

        CmdReply(LocalPlayer.Order);
    }

    [SyncVar] int firstOrder = 0;
    public static int FirstOrder => instance.firstOrder;

    /// <summary>
    /// ���ο� ���带 �����ϱ� ���� ���ʸ� ó������ �ǵ����ϴ�.
    /// </summary>
    [Server]
    public void SetNewRound()
    {
        //���ο� ���带 �����ϱ� ����
        //�̹� ������ ù ��° �÷��̾ ã�´�.
        ResetAcknowledgements();
        ResetHasTurn();

        //������ ù ��° �÷��̾ ��ȿ������ Ȯ��
        if (players[firstOrder] == null || players[firstOrder].isGameOver)
        {
            do
            {
                firstOrder = (firstOrder + (IsClockwise ? 1 : maxPlayer - 1)) % maxPlayer;
            }
            while (players[firstOrder] == null || players[firstOrder].isGameOver);
        }

        SetCurrentOrder(firstOrder);
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

    [SyncVar]
    public bool IsClockwise = true;

    #endregion


    [Server]
    public bool RoundFinished()
    {
        foreach (Player p in instance.players)
        {
            if (p == null || p.isGameOver) continue;

            //���ʸ� ������ ���� �÷��̾ �ִٸ�...
            if (!p.hasTurn)
            {
                return false;
            }
        }

        ResetHasTurn();
        return true;
    }

    [Server]
    public void ResetHasTurn()
    {
        foreach (Player p in instance.players)
        {
            if (p != null)
                p.hasTurn = false;
        }
    }

    [Server]
    public void EndTurn(int order)
    {
        //������ ���� ���
        if (AliveCount == 1)
        {
            CurrentPhase = GamePhase.EndPhase;
            RpcEndGame(LastPlayer());
            return;
        }

        //��� �� ���� ���ʸ� ���� ��� => ���� ���� �ð�!
        if (RoundFinished())
        {
            firstOrder = NextOrder(firstOrder);
            deck.ServerStartDraftPhase();
        }
        else
        {
            //���� ���ʰ� ���� ���
            SetCurrentOrder(NextOrder(order));
            GetPlayer(currentOrder).RpcStartTurn();
        }
    }

    [ClientRpc]
    void RpcEndGame(int lastPlayerOrder)
    {
        CurrentPhase = GamePhase.EndPhase;

        CameraController.instance.FocusOnPlayerField(lastPlayerOrder);
        CameraController.instance.MoveLock(true);
        OnEndGame.Invoke();

        if (LocalPlayer.isGameOver)
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("�й�...", 5);
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("����� �¸��Դϴ�!", 5);
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

    #region Json : �� ����Ʈ
    string filePath = "DeckIdList.json";

    public DeckIDList LoadDeckIdList()
    {
        filePath = Path.Combine(Application.persistentDataPath, filePath);

        if (File.Exists(filePath))
        {
            // ������ �����ϸ� ���Ͽ��� JSON�� �а� ��ü�� ��ȯ
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DeckIDList>(json);
        }
        else
        {
            // ������ ������ �⺻ DeckIDList ��ü�� ����
            DeckIDList newDeck = new();
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