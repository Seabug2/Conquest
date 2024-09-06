using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar, SerializeField, Header("게임 오버 상태")]
    bool isGameOver = false;

    public bool IsGameOver => isGameOver;

    [SyncVar(hook = nameof(SetUp))] //새로 할당하려는 값이 기존의 값과 같으면 호출이 안됨
    public int myOrder = -1;

    #region 패, 덱, 필드, 카메라 컨트롤러
    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck deck;
    public Deck Deck => deck;

    CameraController camCtrl;
    #endregion

    private void Start()
    {
        //모든 클라이언트에서 생성된 플레이어 객체는 스스로 GameManage의 Player List에 추가됨
        GameManager.instance.AddPlayer(this);
    }

    public void SetUp(int _, int value)
    {
        myOrder = value;
        isGameOver = false;

        //자신의 필드 찾기
        Field[] fields = FindObjectsOfType<Field>();
        foreach (Field f in fields)
        {
            if (f.seatNum == myOrder)
            {
                field = f;
                break;
            }
        }

        //자신의 핸드 찾기
        Hand[] hands = FindObjectsOfType<Hand>();
        foreach (Hand h in hands)
        {
            if (h.seatNum == myOrder)
            {
                hand = h;
                break;
            }
        }

        deck = FindObjectOfType<Deck>();

        camCtrl = Camera.main.GetComponent<CameraController>();
        camCtrl.Init(value);
    }

    [Command]
    public void CmdStartTurn()
    {
        //차례를 시작하면 가장 먼저 덱에서 카드를 드로우 합니다.

        if (deck.Count == 0)
        {
            Debug.Log("덱이 한 장도 없습니다!");
            return;
        }

        //서버 역할을 하고 있는 클라이언트의 리모트 객체에서 호출
        int drawnID = UnityEngine.Random.Range(0, deck.Count);
        deck.CmdDraw(drawnID); //덱에서 무작위로 빌런이 뽑혔고 List에서 제거됨

        RpcStartTurn(drawnID);
    }

    public event Action OnDrawAction;

    [ClientRpc]
    void RpcStartTurn(int drawnID)
    {
        Hand.AddHand(Deck.Cards[drawnID]); //모든 클라이언트의 리모트 객체의 Hand에 해당 ID를 가진 Villain이 추가
        OnDrawAction?.Invoke();

        camCtrl.SetVCam(myOrder); //지금 차례의 필드로 이동

        Debug.Log($"{myOrder + 1}번째 플레이어의 차례");

        // 자신의 차례를 시작
        // 모든 클라이언트에서 플레이어 객체에서 실행되어야 할 것들...
        // 1) 차례를 알리는 UI Pop Up
        // 카메라가 자신의 화면을 주목하도록 바꿈

        // 2) UI가 사라진 후 로컬 플레이어에서만 드로우 UI가 팝업
        if (!isLocalPlayer) return;

        //로컬 플레이어는 덱에서 카드를 드로우할 수 있도록
        //드로우 UI가 나타난다.

        StartCoroutine(MyTurn());

        //차례를 넘길 수 있는 버튼이 활성화 됨
    }

    /* 연결 해야할 작업
     * 턴 종료 UI 버튼이 활성화 될 수 있도록 연결 해야함
     * 
     */
    public event Action OnStartTurnAction;

    [HideInInspector]
    public bool isMyTurn = false;

    IEnumerator MyTurn()
    {
        camCtrl.SetVCam(myOrder);
        //화면을 옮길 수 없음
        isMyTurn = true;

        OnStartTurnAction?.Invoke();

        //화면을 옮길 수 있음
        while (isMyTurn)
        {
            //자신의 패에서 낼 수 있는 카드가 없을 경우 자신의 차례를 종료함
            //if(자신의 패에서...)
            isMyTurn = false;

            if (!isMyTurn)
            {
                ///////////////여기서 
                yield break;
            }

            //차례를 종료하고 싶으면 
            yield return null;
        }

        camCtrl.SetVCam(myOrder);
        //화면을 옮길 수 없음


    }

    [Command]
    void CmdEndTurn()
    {
        if (hand.IsGameOver())
        {
            isGameOver = true;
            GameManager.instance.GameOver(myOrder);
            return;
        }
        CmdNextTurn();
    }

    [Command]
    public void CmdNextTurn()
    {
        
        print($"{myOrder + 1}번째 플레이어가 차례를 마침");

        // 턴 종료 UI효과
        // 모든 플레이어 화면이동...

        if (isLocalPlayer)
            GameManager.instance.NextTurn(myOrder);
    }


    public void SelectCard()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.GetMouseButtonDown(1));
    }

    IEnumerator SelectCard_co()
    {
        while (true)
        {

            yield return null;
        }
    }
}
