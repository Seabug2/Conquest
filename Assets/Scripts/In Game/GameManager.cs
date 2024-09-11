using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //전역 클래스 설정
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

    [Header("카드 뒷면 이미지"), Space(10)]
    public Sprite cardBackFace;

    [SerializeField]
    [Header("플레이어 오브젝트"), Space(10)]
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

    [Header("참조"), Space(10)]
    [SerializeField] Card[] cards;
    public static Card Card(int id) => instance.cards[id];
    /// <summary>
    /// 게임의 모든 카드 장수 (종류 수)
    /// </summary>
    public static int TotalCard => instance.cards.Length;

    [Header("덱")]
    [SerializeField] Deck deck;
    public Deck Deck => deck;

    [Header("필드")]
    [SerializeField] Field[] fields;
    public static Field Field(int i) =>
        GameManager.instance.fields[i];

    [Header("패")]
    [SerializeField] Hand[] hands;
    public static Hand Hand(int i) => instance.hands[i];

    #region #1 플레이어의 연결과 게임 시작
    //각 클라이언트에 존재하는 GameManager의 List에 Player 객체를 추가
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
        //셔플
        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(i, 4);
            NetworkPlayer tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        //플레이어 객체에 순번을 할당
        for (int i = 0; i < 4; i++)
        {
            players[i].SetUp(i);
        }

        players.Sort((a, b) => a.myOrder.CompareTo(b.myOrder));

        GameStart();
    }

    [Header("클라이언트 연결 완료 이벤트"), Space(10), Tooltip("모든 플레이어가 연결이 되면 실행되는 이벤트")]
    public UnityEvent OnConnectionEvent;

    [Header("게임 시작 이벤트"), Space(10), Tooltip("게임이 시작할 때 실행될 이벤트를 등록합니다.")]
    public UnityEvent OnGameStartEvent;

    [ClientRpc]
    void GameStart()
    {
        //각 Game Manager에서 List의 순서를 정리함
        CurrentOrder = 0;
        //화면 페이드 인 후에, 메시지 출력후에, 게임 시작
        UIMaster.Fade.In(1.5f, () =>
        {
            UIMaster.LineMessage.PopUp("게임 시작", 3f, () =>
            {
                OnGameStartEvent?.Invoke();
            });
        });
    }
    #endregion

    public int CurrentOrder { get; private set; }

    /// <summary>
    /// 순서를 넘길 방향 (true : 오름차순 / false : 내림차순)
    /// </summary>
    public bool isClockwise = true;

    /// <param name="step">현재 진행 방향으로 차례를 넘깁니다.</param>
    /// <returns>임의의 차례 만큼 순서를 넘길 수 있습니다.</returns>
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
        print($"현재 차례를 마친 플레이어 : {players[CurrentOrder]} / 다음 차례의 플레이어 : {nextPlayer}");
        // ClientRpc 속성의 자신의 차례를 실행하는 메서드 실행

        nextPlayer.CmdStartTurn();
    }

    /// <summary>
    /// 다음 차례의 플레이어번호
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
    /// 현재 생존한 플레이어의 수
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
