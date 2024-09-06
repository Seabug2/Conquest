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
            for (int i = 0; i < cards.Length; i++)
            {
                deckRemaining.Add(cards[i].ID);
            }
    }

    /// <summary>
    /// SyncList를 동기화화기 위해서 서버에서만 실행되어야 함
    /// </summary>
    /// <param name="drawnId">덱에서 꺼낼 ID</param>
    [Server]
    public void CmdDraw(int drawnId)
    {
        deckRemaining.Remove(drawnId);

        RpcDraw();
    }

    [ClientRpc]
    void RpcDraw()
    {
        //덱에서 애니메이션 재생
    }

    //인스펙터를 통해 게임매니저의 이벤트에 연결하여 사용
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

    public float range = 1f;

    [ClientRpc]
    void RpcOpenDraftCard(int[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            cards[numbers[i]].positionKeeper.targetPosition = draftZone[i].position;
            cards[numbers[i]].positionKeeper.enabled = true;
        }
    }
}
