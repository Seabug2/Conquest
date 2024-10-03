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
    #region 싱글톤
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

    [Header("덱, 필드, 손 관리")]
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
    [Header("플레이어 연결 이벤트")]
    public UnityEvent ConnectionEvent = new();

    //[Space(20)]
    //[Header("연결 완료 이벤트")]
    //public UnityEvent CompleteConnect = new ();

    [Space(20)]
    [Header("게임 시작 이벤트")]
    public UnityEvent OnStartEvent = new();

    [Header("게임 종료 이벤트")]
    public UnityEvent OnEndGame = new();


    #region 초기화
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

    #region 초기 진행
    //플레이어 4명의 연결을 대기
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

        //호스트가 아닌 서버인 경우에도 동기화 할 수 있도록...
        if (isServerOnly)
        {
            players.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        RpcSortPlayerList();

        await WaitForAllAcknowledgements();

        SyncDeckIdList(deckIds);

        await WaitForAllAcknowledgements();

        //TODO 덱 ID 리스트를 전달
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
            rand = UnityEngine.Random.Range(i, maxPlayer); // i부터 랜덤 인덱스까지 선택
            tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        for (int i = 0; i < maxPlayer; i++)
        {
            //호스트가 아닌 서버에서도 순서를 동기화 할 수 있도록...
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
            Debug.LogError("오류 : 덱 ID 리스트를 불러오는데 실패한 것 같습니다.");
            return;
        }

        NetworkManager manager = NetworkManager.singleton;

        //서버에서 카드를 생성
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

                //id를 참조하여 카드를 세팅...
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
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("최후의 1인이 되세요!", 3f))
            .Add(() => CmdReply(LocalPlayer.Order))
            .Play();
    }
    #endregion

    #region 연결
    readonly bool[] isAcknowledged = new bool[maxPlayer];

    [Command(requiresAuthority = false)]
    public void CmdReply(int order)
    {
        Debug.Log($"{order} 답장 함");

        if (order < 0 || order >= maxPlayer)
        {
            Debug.LogError($"인덱싱 오류 ({order}) : 범위 초과");
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
            Debug.Log("응답 완료!");
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("응답이 오지 않은 플레이어가 있습니다. 연결 상태를 확인합니다.");
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
                Debug.LogError($"Player {i}는 게임을 종료했습니다...");
            }
            else if (player.isGameOver)
            {
                Debug.LogError($"Player {i}는 탈락했으므로 그냥 넘어갑니다.");
            }
            else
            {
                Debug.LogError($"Player {i}의 연결을 끊습니다...");
                // TODO: 응답하지 않는 플레이어에 대한 예외 처리 필요
            }
        }
    }
    #endregion

    #region 플레이어
    public const int maxPlayer = 4;

    //[SerializeField, Header("플레이어 리스트")]
    readonly
        List<Player> players = new();

    public static Player GetPlayer(int i)
    {
        if (i < 0 || i >= maxPlayer)
        {
            Debug.LogError("잘못된 인덱싱");
            return null;
        }
        return instance.players[i];
    }

    public static Player LocalPlayer { get; private set; }

    [Server]
    public void RemovePlayer(Player _player)
    {
        //플레이어 리스트에 추가된 플레이어인 경우
        if (players.Contains(_player))
        {
            //게임을 아직 시작하지 않은 상태라면,
            if (CurrentPhase.Equals(GamePhase.Standby))
            {
                //플레이어 리스트에서 제거하여 리스트의 크기를 원래대로 한다.
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
                //어떤 플레이어가 게임을 종료했을 때
                //남은 유저가 한 명이라면 그 즉시 게임은 끝이 난다.
                RpcEndGame(LastPlayer());
                return;
            }
        }

        //플레이어 리스트에 추가된 플레이어 객체가 아니었을 경우 처리 없음
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
                //클라이언트를 종료했거나,
                //게임 오버된 플레이어
                if (p == null || p.isGameOver)
                {
                    alivePlayerCount--;
                }
            }

            return alivePlayerCount;
        }
    }
    #endregion

    #region 카드
    [Header("카드 뒷면 이미지"), Space(10)]
    public Sprite cardBackFace;

    [Header("카드"), Space(10)]
    [SerializeField] Card prefab;

    /// <summary>
    /// 미리 생성해둔 상태를 참조하여 재활용성을 높입니다.
    /// </summary>
    public readonly ICardState noneState = new NoneState();

    readonly Dictionary<int, Card> dict_Card = new();

    public static Card Card(int id)
    {
        if (!instance.dict_Card.ContainsKey(id))
        {
            Debug.LogError("존재하지 않는 카드입니다");
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
            Debug.LogError("CSV 파일을 찾을 수 없습니다.");
            return Array.Empty<string>();
        }

        string[] dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 부족합니다.");
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
            Debug.LogError($"데이터 라인 인덱스가 범위를 벗어났습니다: {lineIndex}");
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
            Debug.LogError($"이미지를 찾을 수 없습니다: {imageName}");
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
        return Attribute.IsEmpty;  // 변환 실패 시 기본값 반환
    }
    #endregion

    #region 순서
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
    /// 새로운 라운드를 시작하기 위해 차례를 처음으로 되돌립니다.
    /// </summary>
    [Server]
    public void SetNewRound()
    {
        //새로운 라운드를 시작하기 위해
        //이번 라운드의 첫 번째 플레이어를 찾는다.
        ResetAcknowledgements();
        ResetHasTurn();

        //지금의 첫 번째 플레이어가 유효한지를 확인
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

            //차례를 가지지 못한 플레이어가 있다면...
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
        //게임이 끝난 경우
        if (AliveCount == 1)
        {
            CurrentPhase = GamePhase.EndPhase;
            RpcEndGame(LastPlayer());
            return;
        }

        //모두 한 번씩 차례를 가진 경우 => 인재 영입 시간!
        if (RoundFinished())
        {
            firstOrder = NextOrder(firstOrder);
            deck.ServerStartDraftPhase();
        }
        else
        {
            //아직 차례가 남은 경우
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
            UIManager.GetUI<LineMessage>().ForcePopUp("패배...", 5);
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("당신의 승리입니다!", 5);
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
    /// 자신의 앞 차례 플레이어가 발동한 카드의 패 제한 효과를 전부 계산하여 자신의 패 제한 값을 가져옵니다.
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

    #region Json : 덱 리스트
    string filePath = "DeckIdList.json";

    public DeckIDList LoadDeckIdList()
    {
        filePath = Path.Combine(Application.persistentDataPath, filePath);

        if (File.Exists(filePath))
        {
            // 파일이 존재하면 파일에서 JSON을 읽고 객체로 변환
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DeckIDList>(json);
        }
        else
        {
            // 파일이 없으면 기본 DeckIDList 객체를 생성
            DeckIDList newDeck = new();
            SaveDeckIdList(newDeck);  // 기본 DeckIDList를 파일로 저장
            return newDeck;
        }
    }

    // 파일 저장
    public void SaveDeckIdList(DeckIDList deckIdList)
    {
        // JSON 문자열로 변환
        string json = JsonUtility.ToJson(deckIdList, true);

        // 파일로 저장
        File.WriteAllText(filePath, json);

        Debug.Log($"DeckIdList 저장됨: {filePath}");
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
                Debug.LogError("범위 오류");
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