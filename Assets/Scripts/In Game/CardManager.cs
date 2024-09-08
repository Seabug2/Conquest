using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardManager : NetworkBehaviour
{
    [SerializeField] Card[] cards;
    public Card GetCard(int ID) => cards[ID];

    public readonly SyncList<int> deck = new SyncList<int>();
    /// <summary>
    /// 덱에 남은 카드의 수
    /// </summary>
    public int Count => deck.Count;

    [ServerCallback]
    void Shuffle()
    {
        for (int i = 0; i < Count - 1; i++)
        {
            int rand = Random.Range(i, Count); // i부터 Count-1까지의 인덱스를 랜덤으로 선택
            int tmp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = tmp;
        }
    }

    /// <summary>
    /// 덱에서 카드를 무작위로 꺼내는 경우...
    /// </summary>
    [Server]
    public int RandomPickUpID()
    {
        int drawNumber = deck[Random.Range(0, Count)];
        deck.Remove(drawNumber);
        return drawNumber;
    }

    /// <returns>덱 맨 위의 카드 ID를 반환합니다. 덱 맨 위의 카드란, 리스트의 첫 번째 요소입니다.</returns>
    [Server]
    public int DrawCardID()
    {
        int drawNumber = deck[0];
        deck.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    void ReturnID(int id, bool placeOnTop = false)
    {
        //이미 덱 안에 있는 카드라면 다시 덱에 넣을 수 없다.
        if (deck.Contains(id)) return;
        
        // placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        if (Count == 0)
        {
            deck.Add(id); // 덱이 비어있다면 그냥 추가
        }
        else
        {
            deck.Insert(placeOnTop ? 0 : Random.Range(0, Count), id);
        }
    }

    [Server]
    void ReturnID(Card card, bool placeOnTop = false)
    {
        //이미 덱 안에 있는 카드라면 다시 덱에 넣을 수 없다.
        if (deck.Contains(card.ID)) return;
        
        // placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        if (Count == 0)
        {
            deck.Add(card.ID); // 덱이 비어있다면 그냥 추가
        }
        else
        {
            deck.Insert(placeOnTop ? 0 : Random.Range(0, Count), card.ID);
        }
    }


    [SerializeField]
    Transform[] draftZone;

    private void Start()
    {
        if (isServer)
        {
            int length = cards.Length;
            for (int i = 0; i < length; i++)
            {
                deck.Add(i);
            }
            Shuffle();
        }
    }

    /// <param name="drawnId">제거할 카드의 ID</param>
    [Server]
    public void CmdRemove(int drawnId)
    {
        deck.Remove(drawnId);
    }

    /// <param name="drawnId">덱에 추가할 카드의 ID</param>
    [Server]
    public void CmdAdd(int drawnId)
    {
        deck.Add(drawnId);
    }

    //인스펙터를 통해 게임매니저의 이벤트에 연결하여 사용
    [ServerCallback]
    public void DraftPhase()
    {
        //현재 플레이어 수만큼의 Int 배열을 만들고
        int playerCount = GameManager.instance.AliveCount;
        int[] opened = new int[playerCount];

        //덱에서 카드를 4장 뽑아 모두 같이 확인한다.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcOpenDraftCard(opened);
    }

    /// <param name="opened">Card IDs</param>
    [ClientRpc]
    void RpcOpenDraftCard(int[] opened)
    {
        for (int i = 0; i < opened.Length; i++)
        {

            Card c = GetCard(opened[i]);
            c.handler.SetFace(true);

            float x = draftZone[i].position.x;
            float y = draftZone[i].position.y;
            float z = draftZone[i].position.z;

            c.handler.SetPosition(x, y, z, (i + 1) * .18f);
        }
    }
}
