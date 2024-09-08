using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //전역 클래스 설정
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

    [Header("카드 뒷면 이미지"), Space(10)]
    public Sprite cardBackFace;

    [SerializeField]
    [Header("플레이어 오브젝트"), Space(10)]
    List<NetworkPlayer> players = new List<NetworkPlayer>();
    public NetworkPlayer GetPlayer(int i)
    {
        return players[i];
    }

    [Header("플레이어 오브젝트의 참조 목록"), Space(10)]
    public CardManager cardManager;
    public Field[] fields;
    public Hand[] hands;

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
            int rand = Random.Range(0, 4);
            NetworkPlayer tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }

        //플레이어 객체에 순번을 할당
        for (int i = 0; i < 4; i++)
        {
            players[i].SetUp(i);
        }

        PlayerListSetUp();
    }

    [Header("클라이언트 연결 완료 이벤트"), Space(10), Tooltip("모든 플레이어가 연결이 되면 실행되는 이벤트")]
    public UnityEvent OnConnectionEvent;

    [Header("게임 시작 이벤트"), Space(10), Tooltip("게임이 시작할 때 실행될 이벤트를 등록합니다.")]
    public UnityEvent OnGameStartEvent;

    [ClientRpc]
    void PlayerListSetUp()
    {
        //각 Game Manager에서 List의 순서를 정리함
        players.Sort((a, b) => a.myOrder.CompareTo(b.myOrder));

        //화면 페이드 인 후에, 메시지 출력후에, 게임 시작
        UIController.instance.Fade.In(1.5f, () =>
        {
            UIController.instance.LineMessage.PopUp("게임 시작", 3f, () =>
            {
                OnGameStartEvent?.Invoke();
            });
        });
    }
    #endregion

    /// <summary>
    /// i번째 플레이어의 차례를 마치고 i + 1번째 플레이어의 차례를 시작한다.
    /// </summary>
    /// <param name="i">지금 차례를 마친 플레이어의 번호</param>
    [Command]
    public void NextTurn(int i)
    {
        NetworkPlayer nextPlayer = NextOrderPlayer(i);
        print($"현재 차례를 마친 플레이어 : {players[i]} / 다음 차례의 플레이어 : {nextPlayer}");
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

    /// <summary>
    /// i번째 플레이어가 패 제한 수를 초과할 때 호출
    /// </summary>
    /// <param name="i">탈락한 플레이어의 번호</param>
    [Server]
    public void GameOver(int i)
    {
        if (AliveCount == 1)
        {
            NetworkPlayer winner = players.SingleOrDefault(p => p.IsGameOver == false);
            Debug.Log($"승리 : {winner.connectionToServer}");
            ////모든 플레이어의 카메라가 승리한 플레이어의 필드로 이동
            FocusToWinner(winner.myOrder);
            return;
        }

        NextTurn(i);
    }

    [ClientRpc]
    void FocusToWinner(int winnerNumber)
    {
        CameraController.instance.SetVCam(winnerNumber);
    }
}
