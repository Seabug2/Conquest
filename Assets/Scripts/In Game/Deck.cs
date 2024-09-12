using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 역할 : 덱에 있는 카드의 ID를 리스트로 저장하고있는 클래스
/// </summary>
public class Deck : NetworkBehaviour
{
    public readonly SyncList<int> list = new SyncList<int>();

    /// <summary>
    /// 덱에 남은 카드의 수
    /// </summary>
    public int Count => list.Count;


    [Server]
    void Shuffle()
    {
        for (int i = 0; i < Count - 1; i++)
        {
            int rand = Random.Range(i, Count); // i부터 Count-1까지의 인덱스를 랜덤으로 선택
            int tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }

    /// <returns>덱 맨 위의 카드 ID를 반환합니다. 덱 맨 위의 카드란, 리스트의 마지막 요소입니다.</returns>
    [Server]
    public int DrawCardID(bool isTopCard = true)
    {
        int drawNumber = isTopCard ? list[Count - 1] : Random.Range(0, Count);
        list.Remove(drawNumber);
        return drawNumber;
    }

    [Server]
    Deck ReturnCard(int id, bool placeOnTop = false)
    {
        //이미 덱 안에 있는 카드라면 다시 덱에 넣을 수 없다.
        if (list.Contains(id)) return this;
        if (id < 0 || id >= GameManager.TotalCard) return this;

        //placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        list.Add(id);

        if (!placeOnTop)
            Shuffle();

        return this;
    }

    private void Start()
    {
        if (isServer)
        {
            int length = GameManager.TotalCard;
            for (int i = 0; i < length; i++)
            {
                list.Add(i);
            }
            Shuffle();
        }
    }

    [SerializeField, Header("인재 영입 시간에 카드를 내는 위치")]
    Transform[] draftZone;

    //서버에서만 실행
    [ServerCallback]
    public void ServerDraftPhase()
    {
        if (Count.Equals(0))
        {
            Debug.Log("덱에 카드가 없습니다!");
            //게임 종료??
            return;
        }

        //현재 플레이어 수만큼의 Int 배열을 만들고
        int playerCount =  GameManager.AliveCount;
        int[] opened = new int[playerCount];

        //덱에서 카드를 4장 뽑아 모두 같이 확인한다.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcDraftPhase(opened);
    }

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        for (int i = 0; i < opened.Length; i++)
        {
            Card c = GameManager.Card(opened[i]);
            c.IsOpened = true;
            c.handler.SetPosition(draftZone[i].position);
            c.handler.DoMove(i * .18f);
        }

        //"인재 영입 시간" 메시지 출력
        //딜레이 시간 필요
        //지속 시간 필요
        //메시지 출력을 종료시
        // => 로컬 플레이어의 순서가 0인 경우 카드를 고르도록...
    }
}
