using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 역할 : 덱에 있는 카드의 ID를 리스트로 저장하고있는 클래스
/// </summary>
public class Deck : NetworkBehaviour
{
    private void Start()
    {
        GameManager.deck = this;
    }

    public readonly SyncList<int> list = new();

    /// <summary>
    /// 덱에 남은 카드의 수
    /// </summary>
    public int Count => list.Count;

    public override void OnStartServer()
    {
        int length = GameManager.TotalCard;
        for (int i = 0; i < length; i++)
        {
            list.Add(i);
        }
        Shuffle();
    }

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

    [Server]
    /// <returns>덱 맨 위의 카드 ID를 반환합니다. 덱 맨 위의 카드란, 리스트의 마지막 요소입니다.</returns>
    public int DrawCardID(bool isTopCard = true)
    {
        if (Count.Equals(0))
        {
            Debug.LogError("덱에 카드가 없습니다!");
            return -1;
        }
        int drawNumber = isTopCard ? list[Count - 1] : Random.Range(0, Count);
        list.Remove(drawNumber);
        return drawNumber;
    }

    /// <summary>
    /// _placeOnTop가 true라면 카드를 덱 맨 위에 둔다.
    /// _placeOnTop가 false라면 덱에 카드를 넣고 섞는다.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdReturnCard(int _id, bool _placeOnTop)
    {
        //이미 덱 안에 있는 카드라면 다시 덱에 넣을 수 없다.
        if (list.Contains(_id))
        {
            Debug.LogError("이미 덱에 있는 카드를 추가하려고 했습니다. 잘못된 상황입니다.");
            return;
        }
        if (_id < 0 || _id >= GameManager.TotalCard)
        {
            Debug.LogError("잘못된 카드 ID입니다. 잘못된 상황입니다.");
            return;
        }

        list.Add(_id);

        //placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        if (!_placeOnTop) Shuffle();

        RpcReturnCard(_id);
    }

    [ClientRpc]
    void RpcReturnCard(int _id)
    {
        Card card = GameManager.Card(_id);
        card.iCardState = new InDeckState(card);
        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove();
    }


    //서버에서만 실행
    [ServerCallback]
    public void ServerDraftPhase()
    {
        if (Count.Equals(0))
        {
            Debug.LogError("덱에 카드가 없습니다!");
            //게임 종료??
            return;
        }

        //현재 플레이어 수만큼의 Int 배열을 만들고
        int playerCount = GameManager.AliveCount;
        int[] opened = new int[playerCount];

        //덱에서 카드를 4장 뽑아 모두 같이 확인한다.
        for (int i = 0; i < playerCount; i++)
        {
            int id = DrawCardID();
            opened[i] = id;
        }

        RpcDraftPhase(opened);
    }

    [SerializeField, Header("인재 영입의 시간에 카드를 내는 위치"), Space(10)]
    Transform[] draftZone;
    readonly List<Card> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        //리스트를 재사용 할 땐, 재할당이 아니라 Clear()
        draftCard.Clear();

        Commander commander = new();
        commander
            .Add(() => UIMaster.Message.PopUp("인재 영입 시간!", 3f), 1f)
            .Add_While(() =>
            {
                for (int i = 0; i < opened.Length; i++)
                {
                    Card c = GameManager.Card(opened[i]);
                    draftCard.Add(c);
                    c.iCardState = new NoneState();
                    c.IsOpened = true;
                    c.SetTargetPosition(draftZone[i].position);
                    c.SetTargetQuaternion(Quaternion.identity);

                    c.DoMove(i * .18f);
                }
            }, UIMaster.Message.IsPlaying)
            .Add_While(() =>
            {
                //if (GameManager.LocalPlayer.order == 0)
                //{
                //    UIMaster.Message.PopUp("패로 가져갈 카드를 한 장 고르세요", 3f);
                //    foreach (Card card in draftCard)
                //    {
                //        card.iCardState = new SelectionState(card, () => UIMaster.Confirm.PopUp(() =>
                //        {
                //            draftCard.Remove(card);

                //            GameManager.LocalPlayer.hand.Add(card.id);

                //            foreach (Card card in draftCard)
                //            {
                //                card.iCardState = new NoneState();
                //            }

                //        }, "이 카드를 패로 가져갑니다?", card.front));
                //    }
                //}
                //else
                //{
                //    UIMaster.Message.PopUp($"{GameManager.instance.CurrentOrder + 1}번째 플레이어가 카드를 고릅니다.", 3f);
                //}
            }, UIMaster.Message.IsPlaying)
            .Play();
    }


}
