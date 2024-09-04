using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //싱글톤
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

    Dictionary<int, NetworkPlayer> player_D = new Dictionary<int, NetworkPlayer>();

    /// <summary>
    /// 현재 생존한 플레이어의 수
    /// </summary>
    public int PlayerCount { get { return player_D.Count; } }

    public void AddPlayer(NetworkPlayer player)
    {
        player_D.Add(PlayerCount, player);

        if (PlayerCount == NetworkManager.singleton.maxConnections)
        {
            CmdTurnSetup();
        }
        else
        {
            //대기 중...
            return;
        }
    }

    //GameManager의 Authority가 없음
    [Command(requiresAuthority = false)]
    void CmdTurnSetup()
    {
        int[] order = { 0, 1, 2, 3 };

        //셔플
        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(0, 4);
            int tmp = order[i];
            order[i] = order[rand];
            order[rand] = tmp;
        }

        for (int i = 0; i < 4; i++)
        {
            player_D[i].myOrder = order[i];
        }
    }

    /// <summary>
    /// i번째 플레이어의 차례를 마치고 i + 1번째 플레이어의 차례를 시작한다.
    /// </summary>
    /// <param name="i">지금 차례를 마친 플레이어의 번호</param>
    [Command]
    public void NextTurn(int i)
    {
        NetworkPlayer nextPlayer = NextOrderPlayer(i);
        print($"현재 차례를 마친 플레이어 : {player_D[i]} / 다음 차례의 플레이어 : {nextPlayer}");
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
        } while (!player_D.ContainsKey(nextOrder));

        return player_D[nextOrder];
    }

    public int GetAdjacentPlayer(int currentOrder, int dir)
    {
        int adjacentPlayerNumber = currentOrder;
        do
        {
            adjacentPlayerNumber = Mathf.Abs(adjacentPlayerNumber + dir) % 4;
        } while (!player_D.ContainsKey(adjacentPlayerNumber));

        return adjacentPlayerNumber;
    }

    /// <summary>
    /// i번째 플레이어가 패 제한 수를 초과할 때 호출
    /// </summary>
    /// <param name="i"></param>
    [Command]
    public void GameOver(int i)
    {
        //Dictionary에서 제거
        player_D.Remove(i);
        if (PlayerCount == 1)
        {
            NetworkPlayer p = player_D.Values.First();
            int winner = p.myOrder;
            print($"승리 : {p} / {winner}");
            //모든 플레이어의 카메라가 승리한 플레이어의 필드로 이동
            FocusToWinner(winner);
            return;
        }
        NextTurn(i);
    }

    [ClientRpc]
    void FocusToWinner(int i)
    {
        //CameraController.instance.
    }
}
