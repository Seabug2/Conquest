using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 역할 : 덱에 있는 카드의 ID를 리스트로 저장하고있는 클래스
/// </summary>
public class Deck : NetworkBehaviour
{
    readonly Commander commander = new();
    private void OnDestroy()
    {
        commander.Cancel();
    }


    public SyncList<int> list = new();

    public void SetUpDeck(int[] ids)
    {
        int length = ids.Length;
        for (int i = 0; i < length; i++)
        {
            list.Add(ids[i]);
        }
        Shuffle();
    }

    public int Count => list.Count;

    #region 덱 관리
    [Server]
    public void Shuffle()
    {
        int rand;
        int tmp;
        for (int i = 0; i < Count - 1; i++)
        {
            rand = UnityEngine.Random.Range(i, Count);
            tmp = list[i];
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
        GameManager.Card(_id).RpcReturnToDeck(0);

        //placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        if (!_placeOnTop) Shuffle();

    }

    [Command(requiresAuthority = false)]
    public void CmdReturnCard(int[] _ids, bool _shuffle)
    {
        int length = _ids.Length;

        if (length == 0) return;

        for (int i = 0; i < length; i++)
        {
            //이미 덱 안에 있는 카드라면 다시 덱에 넣을 수 없다.
            if (list.Contains(_ids[i]))
            {
                Debug.LogError("이미 덱에 있는 카드를 추가하려고 했습니다. 잘못된 상황입니다.");
                continue;
            }

            if (null != GameManager.Card(_ids[i]))
            {
                list.Add(_ids[i]);
                GameManager.Card(_ids[i]).RpcReturnToDeck(0.123f * (i + 1));
            }
            else
            {
                Debug.LogError("잘못된 카드 ID입니다. 잘못된 상황입니다.");
                continue;
            }
        }

        //placeOnTop이 true면 덱의 맨 위에, false면 랜덤 위치에 삽입
        if (_shuffle) Shuffle();
    }
    #endregion






    #region 인재영입시간
    readonly SyncList<int> draftCard = new();

    [Server]
    public void ServerStartDraftPhase()
    {
        if (isServerOnly)
            GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        //10초 대기...
        float t = 10;

        commander
            .Refresh()
            .Add(GameManager.instance.SetNewRound)
            .WaitUntil(() => GameManager.instance.IsAllReceived() || t <= 0)
            .Add(() =>
            {
                if (!GameManager.instance.IsAllReceived())
                    GameManager.instance.CheckDisconnectedPlayers();

                GameManager.instance.ResetAcknowledgements();

                if (Count == 0)
                {
                    Debug.LogError("덱에 카드가 없습니다!");
                    //TODO 덱에 카드가 없는 상황?
                    return;
                }

                draftCard.Clear();

                //현재 생존한 플레이어 수 만큼 카드를 공개한다.
                int playerCount = Mathf.Min(GameManager.AliveCount, Count);
                int[] drawnCard = new int[playerCount];
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

    public Vector3 draftPositionOffset = new Vector3(0, -5, 0);

    [ClientRpc]
    void RpcDraftPhase(int[] _draftCard)
    {
        GameManager.instance.CurrentPhase = GamePhase.DraftPhase;

        Func<bool> isPlaying = UIManager.GetUI<LineMessage>().IsPlaying;

        commander
            .Refresh()
            .WaitWhile(isPlaying)
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
                    c.SetTargetPosition(transform.position + position + draftPositionOffset);
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

    [Client]
    void ClientSelectDraftCard(int _order)
    {
        CameraController.instance.FocusOnCenter();
        CameraController.instance.MoveLock(true);

        if (UIManager.GetUI<Timer>().IsPlaying())
            UIManager.GetUI<Timer>().@Reset();

        if (GameManager.GetPlayer(_order).isLocalPlayer)
        {
            Selecting(_order);
        }
        else
        {
            Waiting(_order);
        }
    }

    void Selecting(int _order)
    {
        commander
            .Refresh()
            .Add(() =>
            UIManager.GetUI<LineMessage>().PopUp("패로 가져갈 카드를 한 장 고르세요", 3f)
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

                            CmdSelectDraftCard(_order, card.id);
                        }, "이 카드를 패로 가져갑니다?"
                        , card.Front));
                }

                //30초
                UIManager.GetUI<Timer>().Play(30f, () => RandomSelection(_order));
            })
            .Play();
    }

    void RandomSelection(int _order)
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

    void Waiting(int _order)
    {
        commander
            .Refresh()
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp($"{_order + 1}번째 플레이어가\n카드를 고르는 중입니다.", 3f)
            , 3f)
            .Add(() =>
            {
                CameraController.instance.MoveLock(false);
                UIManager.GetUI<Timer>().Play(30f);
            })
            .Play();
    }


    [Command(requiresAuthority = false)]
    void CmdSelectDraftCard(int order, int id)
    {
        draftCard.Remove(id);
        GameManager.GetPlayer(order).hasTurn = true;
        GameManager.GetPlayer(order).Hand.RpcAdd(id);

        order = GameManager.instance.NextOrder(order);
        GameManager.instance.SetCurrentOrder(order);

        //TODO 남은 카드를 기준으로 드래프트 페이즈를 마치면 안됨

        //1. 남은 카드가 더 있지만 현재 차례를 진행할 수 있는 플레이어가 없는 경우...
        //if (GameManager.instance.RoundFinished())
        //{

        //}

        //2. 
        if (draftCard.Count > 1)
        {
            RpcSelectDraftCard(order);
        }
        //3. 남은 카드가 한 장일 경우
        else
        {
            //남은 한 장을 다음 플레이어의 패에 추가
            int lastID = draftCard[0];
            draftCard.Clear();
            GameManager.GetPlayer(order).Hand.RpcAdd(lastID);
            ServerEndDraftPhase();
        }
    }

    [Server]
    void ServerEndDraftPhase()
    {
        float t = 10f;
        commander
            .Refresh()
            .Add(GameManager.instance.SetNewRound)
            .WaitUntil(() => GameManager.instance.IsAllReceived() || t <= 0)
            .Add(() =>
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
        UIManager.GetUI<Timer>().Reset();
        UIManager.GetUI<LineMessage>().ForcePopUp("카드 선택을 마쳤습니다!", 3f);
        GameManager.GetPlayer(_firstOrder).ClientStartTurn();
    }

    #endregion
}
