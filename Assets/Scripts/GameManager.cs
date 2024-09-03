using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //�̱���
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

    Dictionary<int, Villain> player_D = new Dictionary<int, Villain>();

    /// <summary>
    /// ���� ������ �÷��̾��� ��
    /// </summary>
    public int PlayerCount { get { return player_D.Count; } }

    public void AddPlayer(Villain player)
    {
        print(PlayerCount);
        player_D.Add(PlayerCount, player);
        print(PlayerCount);
        //players.Add(player);
        if (PlayerCount == NetworkManager.singleton.maxConnections)
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
            player_D[i].SetUp(order[i]);
        }
    }

    /// <summary>
    /// i��° �÷��̾��� ���ʸ� ��ġ�� i + 1��° �÷��̾��� ���ʸ� �����Ѵ�.
    /// </summary>
    /// <param name="i">���� ���ʸ� ��ģ �÷��̾��� ��ȣ</param>
    [Command]
    public void NextTurn(int i)
    {
        Villain nextPlayer = player_D[NextOrder(i)];
        print($"���� ���ʸ� ��ģ �÷��̾� : {player_D[i]} / ���� ������ �÷��̾� : {nextPlayer}");
    }

    int NextOrder(int i)
    {
        int nextOrder = i;
        do
        {
            nextOrder = (nextOrder + 1) % PlayerCount;
        } while (!player_D.ContainsKey(nextOrder));

        return nextOrder;
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
