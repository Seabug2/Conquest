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
    [Server]
    public void ServerDraftPhase()
    {
        //현재 생존한 플레이어 수 만큼 카드를 공개한다.
        int playerCount = GameManager.AliveCount;
        int[] drawnCard = new int[playerCount];
        draftCard.Clear();

        if (Count == 0)
        {
            Debug.LogError("덱에 카드가 없습니다!");
        }
        else
        {
            for (int i = 0; i < playerCount; i++)
            {
                int id = DrawCardID();
                draftCard.Add(id);
                drawnCard[i] = id;
            }
        }

        GameManager.instance.SetFirstPlayer();

        float t = 10;
        Commander commander = new();
        commander
            .WaitUntil(() => GameManager.instance.IsAllReceived)
            .OnUpdate(() =>
            {
                t -= Time.deltaTime;
                if (t > 0) return;
                GameManager.instance.CheckDisconnectedPlayers();
                commander.Cancel();
            })
            .OnCompleteAll(() =>
            {
                RpcDraftPhase(drawnCard, GameManager.CurrentOrder);
                GameManager.instance.ResetAcknowledgements();
            })
            .Play();
    }

    readonly SyncList<int> draftCard = new();

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard, int firstPlayerOrder)
    {
        //플레이어들의 화면을 센터로 이동시키고 카메라 이동을 불가능하게 한다
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);

        Commander commander = new();
        commander
            .Add(() => UIManager.Message.PopUp("인재 영입 시간!", 3f), 1f)
            .Add(() =>
            {
                int count = _draftCard.Length;
                float intervalAngle = 360f / count;
                float angle = Random.Range(0f, 90f);

                for (int i = 0; i < count; i++)
                {
                    Card c = GameManager.Card(_draftCard[i]);
                    //매번 생성하지 말고 싱글톤에 캐싱한 상태를 사용
                    c.iCardState = GameManager.instance.noneState;
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
            .Add(() => ClientSelectDraftCard(firstPlayerOrder))
            .Play();
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        ClientSelectDraftCard(_order);
    }

    [Client]
    void ClientSelectDraftCard(int _order)
    {
        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            UIManager.Message.PopUp("패로 가져갈 카드를 한 장 고르세요", 3f);

            foreach (int id in draftCard)
            {
                Card card = GameManager.Card(id);

                card.iCardState = new SelectionState(card
                    , () => UIManager.Confirm.PopUp(() =>
                    {
                        //선택 즉시 모든 카드를 선택 불가능한 상태로 바꾼다. 
                        foreach (int id in draftCard)
                        {
                            Card c = GameManager.Card(id);
                            c.iCardState = GameManager.instance.noneState;
                        }
                        CmdSelectDraftCard(card.id);
                    }, "이 카드를 패로 가져갑니다?"
                    , card.front));
            }
        }
        else
        {
            UIManager.Message.ForcePopUp($"{_order + 1}번째 플레이어가\n카드를 고르는 중입니다.", 3f);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard(int id)
    {
        //서버에서의 처리
        draftCard.Remove(id);

        GameManager.GetPlayer(GameManager.CurrentOrder).Hand.RpcAdd(id);

        if (draftCard.Count > 1)
        {
            GameManager.CurrentOrder = GameManager.instance.NextOrder(GameManager.CurrentOrder);
            RpcSelectDraftCard(GameManager.instance.NextOrder(GameManager.CurrentOrder));
        }
        else
        {
            int lastID = draftCard[0];
            GameManager.GetPlayer(GameManager.CurrentOrder).Hand.RpcAdd(lastID);

            GameManager.instance.SetFirstPlayer();

            float t = 10f;
            Commander commander = new();
            commander
                .WaitUntil(() => GameManager.instance.IsAllReceived)
                .OnUpdate(() =>
                {
                    t -= Time.deltaTime;
                    if (t > 0) return;
                    GameManager.instance.CheckDisconnectedPlayers();
                    commander.Cancel();
                })
                .OnComplete(() =>
                {
                    GameManager.instance.ResetAcknowledgements();
                    RpcEndSelectionDraftCard();
                })
                .Play();
        }
    }

    [ClientRpc]
    void RpcEndSelectionDraftCard()
    {
        Commander commander = new Commander()
            .Add(() => UIManager.Message.ForcePopUp("카드 선택을 마쳤습니다!", 3f))
            .WaitSeconds(3.3f)
            .Add(GameManager.GetPlayer(GameManager.CurrentOrder).ClientStartTurn);
        commander.Play();
    }

    #endregion
}
