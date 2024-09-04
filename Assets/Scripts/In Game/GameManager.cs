using System.Linq;
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

    Dictionary<int, NetworkPlayer> player_D = new Dictionary<int, NetworkPlayer>();

    /// <summary>
    /// ���� ������ �÷��̾��� ��
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
            //��� ��...
            return;
        }
    }

    //GameManager�� Authority�� ����
    [Command(requiresAuthority = false)]
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
            player_D[i].myOrder = order[i];
        }
    }

    /// <summary>
    /// i��° �÷��̾��� ���ʸ� ��ġ�� i + 1��° �÷��̾��� ���ʸ� �����Ѵ�.
    /// </summary>
    /// <param name="i">���� ���ʸ� ��ģ �÷��̾��� ��ȣ</param>
    [Command]
    public void NextTurn(int i)
    {
        NetworkPlayer nextPlayer = NextOrderPlayer(i);
        print($"���� ���ʸ� ��ģ �÷��̾� : {player_D[i]} / ���� ������ �÷��̾� : {nextPlayer}");
        // ClientRpc �Ӽ��� �ڽ��� ���ʸ� �����ϴ� �޼��� ����

        nextPlayer.CmdStartTurn();
    }

    /// <summary>
    /// ���� ������ �÷��̾��ȣ
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
    /// i��° �÷��̾ �� ���� ���� �ʰ��� �� ȣ��
    /// </summary>
    /// <param name="i"></param>
    [Command]
    public void GameOver(int i)
    {
        //Dictionary���� ����
        player_D.Remove(i);
        if (PlayerCount == 1)
        {
            NetworkPlayer p = player_D.Values.First();
            int winner = p.myOrder;
            print($"�¸� : {p} / {winner}");
            //��� �÷��̾��� ī�޶� �¸��� �÷��̾��� �ʵ�� �̵�
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
