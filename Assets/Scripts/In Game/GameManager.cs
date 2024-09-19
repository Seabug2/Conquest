using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.IO;

public class GameManager : NetworkBehaviour
{

    //전역 클래스 설정
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


    [Header("카드 뒷면 이미지"), Space(10)]
    public Sprite cardBackFace;


    [Header("플레이어")]
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
            Debug.Log("범위가 잘못됐습니다.");
            return null;
        }

        return instance.players[i];
    }

    [Header("카드"), Space(10)]
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
            Debug.LogError("CSV 파일을 찾을 수 없습니다.");
            return;
        }

        dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 부족합니다.");
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

            //카드 ID 부여
            cards[i].id = int.Parse(data[0]);

            // 이미지를 로드하여 카드의 front에 할당
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

            cards[i].cardName = data[2];
            
            cards[i].Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            cards[i].Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            cards[i].Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            cards[i].Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));
        }
    }

    // string 값을 int로 변환한 후 enum으로 변환하는 함수
    Attribute ParseAttribute(string value)
    {
        if (int.TryParse(value, out int result) && System.Enum.IsDefined(typeof(Attribute), result))
        {
            return (Attribute)result;
        }
        return Attribute.isEmpty;  // 변환 실패 시 기본값 반환
    }

    [Header("덱")]
    [SerializeField] Deck deck;
    public static Deck Deck => instance.deck;

    [Header("필드")]
    [SerializeField] Field[] fields;
    public static Field Field(int i) =>
        GameManager.instance.fields[i];

    [Header("패")]
    [SerializeField] Hand[] hands;
    public static Hand Hand(int i) => instance.hands[i];




    #region #1 플레이어의 연결과 게임 시작
    //각 클라이언트에 존재하는 GameManager의 List에 Player 객체를 추가
    public void AddPlayer(Player player)
    {
        players.Add(player);

        if (players.Count == 4)
        {
            ServerGameStart();
        }
    }

    [Header("클라이언트 연결 완료 이벤트"), Space(10), Tooltip("모든 플레이어가 연결이 되면 실행되는 이벤트")]
    public UnityEvent OnConnectionEvent;

    [Header("게임 시작 이벤트"), Space(10), Tooltip("게임이 시작할 때 실행될 이벤트를 등록합니다.")]
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

        // 플레이어 객체에 순번을 할당
        for (int i = 0; i < 4; i++)
        {
            players[i].order = i;
        }
    }

    [ClientRpc]
    void RpcGameStart()
    {
        //각 Game Manager에서 List의 순서를 정리함
        players.Sort((a, b) => a.order.CompareTo(b.order));

        CurrentOrder = 0;

        Commander
            .Clear()
            .Add_While(() => UIMaster.Fade.In(1.5f), UIMaster.Fade.IsPlaying)
            .WaitSeconds(1f)
            .Add_While(() => UIMaster.Message.PopUp("게임 시작", 3f), UIMaster.Message.IsPlaying)
            .Add(() => OnGameStartEvent?.Invoke())
            .Play();
    }
    #endregion



    public int CurrentOrder { get; private set; }



    /// <summary>
    /// true : 오름차순으로 게임이 진행됩니다.
    /// false : 내림차순으로 게임이 진행됩니다.
    /// </summary>
    public bool isClockwise = true;

    /// <summary>
    /// 다음 차례 순서를 반환
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
    /// 이전 차례 순서를 반환
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
    /// 현재 생존한 플레이어의 수
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
    /// 모든 플레이어가 자신의 차례를 가졌으면 한 라운드가 끝이 난다.
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
}