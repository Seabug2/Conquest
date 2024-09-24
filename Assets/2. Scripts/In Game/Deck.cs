using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 역할 : 덱에 있는 카드의 ID를 리스트로 저장하고있는 클래스
/// </summary>
public class Deck : NetworkBehaviour
{
    public readonly SyncList<int> list = new();

    #region 덱 초기화
    private void Start()
    {
        GameManager.deck = this;
    }

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
    #endregion


    #region 덱 관리
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
    #endregion






    #region 인재영입시간
    [ServerCallback]
    public void ServerDraftPhase()
    {
        //현재 플레이어 수만큼의 Int 배열을 만들고
        int playerCount = GameManager.AliveCount;
        int[] opened = new int[playerCount];

        if (Count == 0)
        {
            Debug.LogError("덱에 카드가 없습니다!");
        }
        else
        {
            //덱에서 카드를 4장 뽑아 모두 같이 확인한다.
            for (int i = 0; i < playerCount; i++)
            {
                int id = DrawCardID();
                opened[i] = id;
            }
        }

        RpcDraftPhase(opened);
    }

    readonly List<Card> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] opened)
    {
        //리스트를 재사용 할 땐, 재할당이 아니라 Clear()
        draftCard.Clear();

        Commander commander = new();
        commander
            .Add(() => UIManager.Message.PopUp("인재 영입 시간!", 3f), 1f)
            .Add(() =>
            {
                int count = opened.Length;
                float intervalAngle = 360f / count;
                float angle = Random.Range(0, 90);

                for (int i = 0; i < opened.Length; i++)
                {
                    Card c = GameManager.Card(opened[i]);
                    draftCard.Add(c);
                    c.iCardState = new NoneState();
                    c.IsOpened = true;

                    angle += intervalAngle;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * 1.5f;
                    c.SetTargetPosition(transform.position + position);
                    c.SetTargetQuaternion(Quaternion.Euler(0, 0, Random.Range(-8f, 8f))); //10도 이상으로 회전하면 어색하게 보임

                    c.DoMove(i * .18f);
                }
            })
            .WaitWhile(UIManager.Message.IsPlaying)
            .Add(StartCardSelection)
            .Play();
    }

    [ServerCallback]
    void StartCardSelection()
    {
        int firstOrder = GameManager.instance.FirstOrder();
        RpcSelectDraftCard(firstOrder);
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard()
    {
        if (GameManager.RoundFinished)
        {
            //Todo 플레이어 개인 턴 시작
            //GameManager.instance.
        }
        else
        {
            RpcSelectDraftCard(GameManager.instance.NextOrder(GameManager.instance.CurrentOrder));
        }
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        GameManager.instance.CurrentOrder = _order;

        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            UIManager.Message.PopUp("패로 가져갈 카드를 한 장 고르세요", 3f);
            foreach (Card card in draftCard)
            {
                card.iCardState = new SelectionState(card
                    , () => UIManager.Confirm.PopUp(() =>
                    {
                        GameManager.instance.LocalPlayer.Hand.CmdAdd(card.id);
                        draftCard.Remove(card);

                        foreach (Card c in draftCard)
                        {
                            c.iCardState = new NoneState();
                        }

                        CmdSelectDraftCard();
                    }, "이 카드를 패로 가져갑니다?"
                    , card.front));
            }
        }
        else
        {
            UIManager.Message.ForcePopUp($"{GameManager.instance.CurrentOrder + 1}번째 플레이어가 카드를 고르는 중입니다.", 5f);
        }
    }
    #endregion
}
