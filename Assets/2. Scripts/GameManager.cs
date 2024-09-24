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
    #region 싱글톤
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

    #region 초기화
    string filePath;
    DeckIDList deckIdList;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "DeckIdList.json");

        // 파일 로드
        deckIdList = LoadDeckIdList();

        CardSetting();
    }
    #endregion

    #region 플레이어
    public const int maxPlayer = 4;

    [SerializeField, Header("플레이어 리스트")]
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
            //게임을 아직 시작하지 않은 상태라면,
            if (!IsPlaying)
            {
                //플레이어 리스트에서 제거하여 리스트의 크기를 원래대로 한다.
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
            rand = UnityEngine.Random.Range(i, count); // i부터 랜덤 인덱스까지 선택
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
            Debug.LogError("CSV 파일을 찾을 수 없습니다.");
            return;
        }

        string[] dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 부족합니다.");
            return;
        }

        //카드에 효과를 할당하기 위한 매니저
        AbilityManager abilityManager = new AbilityManager();

        for (int i = 0; i < 54; i++)
        {
            cards[i].id = deckIdList[i];
            dict_Card.Add(deckIdList[i], cards[i]);

            int lineIndex = cards[i].id + 1;
            if (lineIndex >= dataLines.Length)
            {
                Debug.LogError($"데이터 라인 인덱스가 범위를 벗어났습니다: {lineIndex}");
                continue;
            }

            string line = dataLines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                Debug.LogWarning($"빈 줄이 발견되었습니다: {lineIndex}");
                continue;
            }



            string[] data = line.Split(',');
            //if (data.Length < 최소한의_데이터_길이)
            //{
            //    Debug.LogError($"데이터가 부족합니다: {lineIndex}");
            //    continue;
            //}



            string imageName = data[1];  // 파일명 생성
            Sprite cardFront = Resources.Load<Sprite>("Card/" + imageName);  // Resources/Card 폴더에서 이미지 로드

            if (cardFront == null)
            {
                cards[i].front = null;
                Debug.LogError($"이미지를 찾을 수 없습니다: {imageName}");
            }
            else
            {
                cards[i].front = cardFront;  // 카드의 front 스프라이트 할당
            }

            cards[i].name = data[2];
            cards[i].cardName = data[2];

            cards[i].Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            cards[i].Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            cards[i].Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            cards[i].Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));


            // TODO 카드 효과 구현을 완료하면 삭제
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
        return Attribute.isEmpty;  // 변환 실패 시 기본값 반환
    }
    #endregion

    #region 순서
    /// <summary>
    /// Rpc를 통해 로컬 플레이어가 차례를 시작하면 currentOrder가 갱신된다.
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
    /// true : 오름차순 0 => 1
    /// false : 내림차순 0 <= 1
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

    #region 진행
    public bool IsPlaying { get; private set; }

    public static bool RoundFinished
    {
        get
        {
            foreach (Player p in instance.players)
            {
                if (p == null) continue;

                //차례를 가지지 못한 플레이어가 있다면...
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
    [Header("게임 시작 이벤트")]
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
            .Add(() => UIManager.Message.ForcePopUp("최후의 1인이 되세요!", 5f), 5.5f)
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
                //드래프트...
                return;
            }
            StartTurn();
        }
        else
        {
            RpcEndGame();
        }
    }

    [Header("게임 종료 이벤트")]
    public UnityEvent OnEndGame;

    [ClientRpc]
    void RpcEndGame()
    {
        OnEndGame?.Invoke();
        IsPlaying = false;

        if (LocalPlayer.isGameOver)
        {
            //패배 이벤트
        }
        else
        {
            //승리 이벤트
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
    public DeckIDList LoadDeckIdList()
    {
        if (File.Exists(filePath))
        {
            // 파일이 존재하면 파일에서 JSON을 읽고 객체로 변환
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DeckIDList>(json);
        }
        else
        {
            // 파일이 없으면 기본 DeckIDList 객체를 생성
            DeckIDList newDeck = new DeckIDList();
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