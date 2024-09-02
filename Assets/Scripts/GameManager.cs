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
    /// ���� ������ �÷��̾��� ��
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
            //��� ��...
            return;
        }
    }

    [Command]
    void CmdTurnSetup()
    {
        int[] order = { 0, 1, 2, 3 };

        //����
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
    /// i��° �÷��̾��� ���ʸ� ��ġ�� i + 1��° �÷��̾��� ���ʸ� �����Ѵ�.
    /// </summary>
    /// <param name="i">���� ���ʸ� ��ģ �÷��̾��� ��ȣ</param>
    [Command]
    public void NextTurn(int i)
    {
        //player_D[i + 1]
    }

    /// <summary>
    /// i��° �÷��̾ �� ���� ���� �ʰ��� �� ȣ��
    /// </summary>
    /// <param name="i"></param>
    [Command]
    public void GameOver(int i)
    {
        //Dictionary���� ����
        player_D.Remove(i);

        NextTurn(i);
    }
}
