using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Deck : NetworkBehaviour
{
    [SerializeField] Card[] cards;
    public Card[] Cards => cards;

    public readonly SyncList<int> deckRemaining = new SyncList<int>();
    public int Count { get { return deckRemaining.Count; } }

    [SerializeField]
    Transform[] draftZone;

    private void Start()
    {
        if (isServer)
            for (int i = 0; i < 54; i++)
            {
                deckRemaining.Add(i);
            }
    }

    //���翵�� �ð� : �巡��Ʈ ��


    /// <summary>
    /// SyncList�� ����ȭȭ�� ���ؼ� ���������� ����Ǿ�� ��
    /// </summary>
    /// <param name="drawnId">������ ���� ID</param>
    [Server]
    public void CmdDraw(int drawnId)
    {
        deckRemaining.Remove(drawnId);

        RpcDraw();
    }

    [ClientRpc]
    void RpcDraw()
    {

    }

    [Server]
    public void DraftTurn()
    {
        int playerCount = GameManager.instance.AliveCount;
        int[] numbers = new int[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            int rand = Random.Range(0, deckRemaining.Count);
            numbers[i] = rand;
            CmdDraw(rand);
        }
        RpcOpenDraftCard(numbers);
    }

    [ClientRpc]
    void RpcOpenDraftCard(int[] numbers)
    {
        for(int i = 0; i < numbers.Length; i++)
        {
            cards[numbers[i]].transform.position = draftZone[i].position;
        }
    }
}
