using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
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

    readonly Dictionary<int, Player> player_D = new Dictionary<int, Player>();

    /// <summary>
    /// 현재 생존한 플레이어의 수
    /// </summary>
    public int playerCount { get{return player_D.Count; } }

    public void AddPlayer(Player player)
    {
        player_D.Add(playerCount, player);

        //players.Add(player);
        if (playerCount == NetworkManager.singleton.maxConnections)
        {
            CmdTurnSetup();
        }
        else
        {
            //대기 중...
            return;
        }
    }

    [Command]
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
            player_D[i].SetUp(i);
        }
    }

    /// <summary>
    /// i번째 플레이어의 차례를 마치고 i + 1번째 플레이어의 차례를 시작한다.
    /// </summary>
    /// <param name="i">지금 차례를 마친 플레이어의 번호</param>
    [Command]
    public void NextTurn(int i)
    {
        //player_D[i + 1]
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

        NextTurn(i);
    }
}
