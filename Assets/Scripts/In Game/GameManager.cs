using System;
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
                instance.commander = new Commander();
            }
            return instance.commander;
        }
    }

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
    [Header("플레이어 리스트"), Space(10)]
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

    /// <returns>i번째 순서의 플레이어</returns>
    public static Player Player(int i) => instance.players[i];

    [Header("참조"), Space(10)]
    [SerializeField] Card[] cards;
    public static Card Card(int id) => instance.cards[id];

    /// <summary>
    /// 게임의 모든 카드 장수 (종류 수)
    /// </summary>
    public static int TotalCard => instance.cards.Length;

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

    public bool isClockwise = true;

    /// <summary>
    /// 자신의 다음 차례 플레이어의 번호를 반환합니다.
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

    public static int GetSidePlayer(int myOrder, int dir)
    {
        int sidePlayerNumber = myOrder;
        do
        {
            sidePlayerNumber = (sidePlayerNumber + dir + 4) % 4;
        } while (!Player(sidePlayerNumber).isGameOver);

        return sidePlayerNumber;
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
}