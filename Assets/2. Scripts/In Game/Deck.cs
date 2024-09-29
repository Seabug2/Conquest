using System;
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
            int rand = UnityEngine.Random.Range(i, Count); // i부터 Count-1까지의 인덱스를 랜덤으로 선택
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
        int drawNumber = isTopCard ? list[Count - 1] : UnityEngine.Random.Range(0, Count);
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
    readonly SyncList<int> draftCard = new();

    [Server]
    public void ServerDraftPhase()
    {
        if (isServerOnly)
            GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        //10초 대기...
        float t = 10;

        Commander commander = new Commander()
            .Add(GameManager.instance.SetNewRound)
            .WaitWhile(() => true)
            .CancelTrigger(() => GameManager.instance.IsAllReceived() || t <= 0)
            .OnCanceled(() =>
            {
                if (!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();

                GameManager.instance.ResetAcknowledgements();

                if (Count == 0)
                {
                    Debug.LogError("덱에 카드가 없습니다!");
                    //TODO
                    return;
                }

                //현재 생존한 플레이어 수 만큼 카드를 공개한다.
                int playerCount = Mathf.Min(GameManager.AliveCount, Count);

                int[] drawnCard = new int[playerCount];

                draftCard.Clear();

                for (int i = 0; i < playerCount; i++)
                {
                    int id = DrawCardID();
                    draftCard.Add(id);
                    drawnCard[i] = id;
                }
                RpcDraftPhase(drawnCard);
            })
            .OnUpdate(() => t -= Time.deltaTime)
            .Play();
    }

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard)
    {
        GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;

        new Commander()
            .Add(() =>
            {
                //플레이어들의 화면을 센터로 이동시키고 카메라 이동을 불가능하게 한다
                CameraController.instance.FocusOnCenter();
                CameraController.instance.MoveLock(true);
                UIManager.GetUI<LineMessage>().PopUp("인재 영입 시간!", 3f);
            }, 1f)
            .Add(() =>
            {
                int count = _draftCard.Length;
                float intervalAngle = 360f / count;
                float angle = UnityEngine.Random.Range(0f, 90f);

                for (int i = 0; i < count; i++)
                {
                    Card c = GameManager.Card(_draftCard[i]);
                    c.iCardState = GameManager.instance.noneState;
                    c.IsOpened = true;

                    angle += intervalAngle;
                    float radian = Mathf.Deg2Rad * angle;
                    Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * 1.5f;
                    c.SetTargetPosition(transform.position + position);
                    c.SetTargetQuaternion(Quaternion.Euler(0, 0, UnityEngine.Random.Range(-6f, 6f))); //10도 이상으로 회전하면 어색하게 보임

                    c.DoMove(i * .18f);
                }
            })
            .WaitWhile(isPlaying)
            .Add(() => ClientSelectDraftCard(GameManager.FirstOrder))
            .Play();
    }

    [ClientRpc]
    void RpcSelectDraftCard(int _order)
    {
        ClientSelectDraftCard(_order);
    }

    void EndSelection(int _order)
    {
        if (UIManager.GetUI<Confirm>().IsActive)
        {
            UIManager.GetUI<Confirm>().Close();
        }

        foreach (int id in draftCard)
        {
            Card c = GameManager.Card(id);
            c.iCardState = GameManager.instance.noneState;
        }

        int rand = UnityEngine.Random.Range(0, draftCard.Count);
        CmdSelectDraftCard(_order, draftCard[rand]);
    }

    [Client]
    void ClientSelectDraftCard(int _order)
    {
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);
        UIManager.GetUI<Timer>().@Reset();

        Commander commander = new();

        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            commander
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print("카드 선택");
                    UIManager.GetUI<LineMessage>().PopUp("패로 가져갈 카드를 한 장 고르세요", 3f);
                }
                , 3f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);

                    //현재 공개된 카드의 상태를 바꿈
                    foreach (int id in draftCard)
                    {
                        Card card = GameManager.Card(id);

                        card.iCardState = new SelectionState(card
                            , () => UIManager.GetUI<Confirm>().PopUp(() =>
                            {
                                UIManager.GetUI<Timer>().Stop();

                                //확인 버튼을 누르면...
                                //선택 즉시 모든 카드를 선택 불가능한 상태로 바꾼다. 
                                foreach (int id in draftCard)
                                {
                                    Card c = GameManager.Card(id);
                                    c.iCardState = GameManager.instance.noneState;
                                }

                                CmdSelectDraftCard(GameManager.LocalPlayer.Order, card.id);
                            }, "이 카드를 패로 가져갑니다?"
                            , card.front));
                    }

                    //30초
                    UIManager.GetUI<Timer>().Play(30f, () => EndSelection(_order));
                })
                .Play();
        }
        else
        {
            commander
                .Add(() =>
                {
                    UIManager.GetUI<HeadLine>().Print("인재 영입 시간");
                    UIManager.GetUI<LineMessage>().ForcePopUp($"{_order + 1}번째 플레이어가\n카드를 고르는 중입니다.", 3f);
                }, 3f)
                .Add(() =>
                {
                    CameraController.instance.MoveLock(false);
                    UIManager.GetUI<Timer>().Play(30f);
                })
                .Play();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard(int order, int id)
    {
        draftCard.Remove(id);
        GameManager.GetPlayer(order).Hand.RpcAdd(id);

        order = GameManager.instance.NextOrder(order);

        if (draftCard.Count > 1)
        {
            GameManager.instance.SetCurrentOrder(order);
            RpcSelectDraftCard(order);
            return;
        }

        //남은 카드가 한 장일 경우
        //남은 한 장을 다음 플레이어의 패에 추가
        int lastID = draftCard[0];
        draftCard.Clear();
        GameManager.GetPlayer(order).Hand.RpcAdd(lastID);

        float t = 10f;
        Commander commander = new();
        commander
            .Add(GameManager.instance.SetNewRound)
            .WaitWhile(() => true)
            .CancelTrigger(() => GameManager.instance.IsAllReceived() || t <= 0)
            .OnCanceled(() =>
            {
                if (!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();

                GameManager.instance.ResetAcknowledgements();

                if (isServerOnly)
                    GameManager.instance.CurrentPhase = GamePhase.PlayerPhase;

                RpcEndSelectionDraftCard(GameManager.FirstOrder);
            })
            .OnUpdate(() => t -= Time.deltaTime)
            .Play();
    }

    [ClientRpc]
    void RpcEndSelectionDraftCard(int _firstOrder)
    {
        GameManager.instance.CurrentPhase = GamePhase.PlayerPhase;
        UIManager.GetUI<Timer>().Stop();

        new Commander()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("카드 선택을 마쳤습니다!", 3f), 3f)
            .Add(GameManager.GetPlayer(_firstOrder).ClientStartTurn)
            .Play();
    }

    #endregion
}
