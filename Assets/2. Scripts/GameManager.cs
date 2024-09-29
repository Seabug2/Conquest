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
    Begin,
    DraftPhase,
    PlayerPhase,
    End
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
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    #region 초기화
    string filePath = "DeckIdList.json";
    DeckIDList deckIdList;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, filePath);
        deckIdList = LoadDeckIdList();
        CardSetting();

        CurrentPhase = GamePhase.Begin;
    }
    #endregion

    #region 연결
    [SerializeField] bool[] isAcknowledged = new bool[maxPlayer];

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
        for (int i = 0; i < maxPlayer; i++)
        {
            isAcknowledged[i] = false;
        }
    }

    [Server]
    public async UniTask WaitForAllAcknowledgements(int timeoutMilliseconds = 20000)
    {
        // CancellationTokenSource 생성
        var cts = new CancellationTokenSource();

        // 설정된 시간 후에 CancellationToken을 취소함
        cts.CancelAfter(timeoutMilliseconds);

        // 설정된 시간 동안 응답을 기다리기 시작한다.
        await UniTask.WaitUntil(IsAllReceived, cancellationToken: cts.Token).SuppressCancellationThrow();

        if (!IsAllReceived())
        {
            Debug.LogWarning("응답이 오지 않은 플레이어가 있습니다. 연결 상태를 확인합니다.");
            CheckDisconnectedPlayers();
        }
        else
        {
            Debug.Log("응답 완료!");
        }

        ResetAcknowledgements();
    }

    [Server]
    public void CheckDisconnectedPlayers()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            //답장을 한 
            if (isAcknowledged[i]) continue;

            Player player = GetPlayer(i);
            if (player == null)
            {
                Debug.LogError($"Player {i}는 게임을 종료했습니다...");
            }
            else if (player.isGameOver)
            {
                // 여기에서 해당 플레이어의 연결 상태에 대한 추가 처리
                Debug.LogError($"Player {i}는 탈락했으므로 그냥 넘어갑니다.");
            }
            else
            {
                //TODO 응답하지 않는 플레이어에 대한 예외처리 필요
                Debug.LogError($"Player {i}의 연결을 끊습니다...");
            }
        }
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

    public static Player LocalPlayer { get; private set; }

    public void AddPlayer(Player _player)
    {
        if (_player.isLocalPlayer)
        {
            LocalPlayer = _player;
        }

        players.Add(_player);

        if (isServer && players.Count == maxPlayer)
        {
            StartGame();
        }
    }

    [Server]
    public void RemovePlayer(Player _player)
    {
        if (players.Contains(_player))
        {
            //게임을 아직 시작하지 않은 상태라면,
            if (!IsPlaying)
            {
                //플레이어 리스트에서 제거하여 리스트의 크기를 원래대로 한다.
                players.Remove(_player);
            }
            else
            {
                int i = players.IndexOf(_player);
                players[i] = null;
            }
        }

        if (isServer)
        {
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
    }

    [Server]
    int LastPlayer()
    {
        int last = -1;

        foreach (Player p in players)
        {
            if (p != null && !p.isGameOver)
            {
                last = p.Order;
            }
        }

        return last;
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
            players[i].RpcSetOrder(i);

            //호스트가 아닌 서버에서도 순서를 동기화 할 수 있도록...
            if (isServerOnly)
            {
                players[i].SetOrder(i);
            }
        }
    }

    [Server]
    async void StartGame()
    {
        PlayerShuffle(); //플레이어의 order를 바꾸면 hook을 통해 Reply()를 받을 수 있다.

        await WaitForAllAcknowledgements();

        RpcSortPlayerList();

        //호스트가 아닌 서버인 경우에도 동기화 할 수 있도록...
        if (isServerOnly)
        {
            players.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        await WaitForAllAcknowledgements();

        IsPlaying = true;

        RpcStartGame();

        await WaitForAllAcknowledgements();

        deck.ServerDraftPhase();
    }

    [ClientRpc]
    void RpcSortPlayerList()
    {
        players.Sort((a, b) => a.Order.CompareTo(b.Order));
        CmdReply(LocalPlayer.Order);
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
    public Card[] cards;

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



            string imageName = data[1];
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
        return Attribute.IsEmpty;  // 변환 실패 시 기본값 반환
    }
    #endregion

    #region 순서
    int currentOrder;

    public static int CurrentOrder => instance.currentOrder;

    [Server]
    public void SetCurrentOrder(int newOrder)
    {
        currentOrder = newOrder;
        RpcSetCurrentOrder(newOrder);
    }

    [ClientRpc]
    public void RpcSetCurrentOrder(int newOrder)
    {
        GetPlayer(currentOrder).isMyTurn = false;

        currentOrder = newOrder;

        GetPlayer(currentOrder).isMyTurn = true;
        GetPlayer(currentOrder).hasTurn = true;

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

    public GamePhase CurrentPhase = GamePhase.Begin;

    public static Deck deck;

    public static Dictionary<int, Field> dict_Field = new();

    public static Dictionary<int, Hand> dict_Hand = new();

    /// <summary>
    /// 미리 생성해둔 상태를 참조하여 재활용성을 높입니다.
    /// </summary>
    public readonly ICardState noneState = new NoneState();

    #region 진행
    [SyncVar]
    public bool IsPlaying = false;

    [Server]
    public bool RoundFinished()
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

    [Space(20)]
    [Header("게임 시작 이벤트")]
    public UnityEvent OnStartEvent;

    [ClientRpc]
    public void RpcStartGame()
    {
        new Commander()
            .Add(() =>
            {
                CameraController.instance.FocusOnCenter();
                UIManager.GetUI<Fade>().In(3f);
            })
            .WaitSeconds(3.3f)
            .Add(() =>
            {
                UIManager.GetUI<Timer>().Active(1.8f);
                UIManager.GetUI<HeadLine>().On(1.8f);
            })
            .WaitSeconds(1.8f)
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("최후의 1인이 되세요!", 3f)
            , 3.3f)
            .Add(() =>
            {
                Debug.Log(LocalPlayer.Order + "의 리플라이");
                CmdReply(LocalPlayer.Order);
            })
            .Play();
    }

    [Server]
    public void EndTurn(int order)
    {
        if (isServerOnly)
        {
            CurrentPhase = GamePhase.End;
        }

        //게임이 끝난 경우
        if (AliveCount == 1)
        {
            IsPlaying = false;
            RpcEndGame(LastPlayer());
            return;
        }

        //모두 한 번씩 차례를 가진 경우 => 인재 영입 시간!
        if (RoundFinished())
        {
            deck.ServerDraftPhase();
            return;
        }

        //아직 차례가 남은 경우
        SetCurrentOrder(NextOrder(order));
        GetPlayer(currentOrder).RpcStartTurn();
    }

    [Header("게임 종료 이벤트")]
    public UnityEvent OnEndGame;

    [ClientRpc]
    void RpcEndGame(int lastPlayerOrder)
    {
        CameraController.instance.CurrentCamIndex = lastPlayerOrder;
        CameraController.instance.MoveLock(true);
        OnEndGame?.Invoke();

        if (LocalPlayer.isGameOver)
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("패배...", 5);
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("당신의 승리입니다!", 5);
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