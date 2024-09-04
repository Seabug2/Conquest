using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

//플레이어 객체
public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetUp))] //새로 할당하려는 값이 기존의 값과 같으면 호출이 안됨
    public int myOrder = -1;

    [SerializeField] Hand hand;
    public Hand Hand => hand;

    [SerializeField] Field field;
    public Field Field => field;

    [SerializeField] Deck deck;
    public Deck Deck => deck;

    CameraController camCtrl;

    private void Start()
    {
        //Host의 씬에 플레이어 오브젝트가 추가될 때마다 GameManager의 Player Dictionary에 오브젝트를 추가한다.
        if (isServer)
            GameManager.instance.AddPlayer(this);
    }

    public void SetUp(int _, int value)
    {
        myOrder = value;

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

        if (isLocalPlayer)
            //자신의 카메라 찾기
            camCtrl.Init(value);

        //자신이 첫 번째 플레이어라면 먼저 차례를 시작한다.
        if (myOrder == 0 && isLocalPlayer)
        {
            CmdStartTurn();
        }
    }

    [Command]
    public void CmdStartTurn()
    {
        if (deck.Count == 0)
        {
            Debug.Log("덱이 한 장도 없습니다!");
            return;
        }

        //서버 역할을 하고 있는 클라이언트의 리모트 객체에서 호출
        int drawnID = Random.Range(0, deck.Count);
        deck.CmdDraw(drawnID); //덱에서 무작위로 빌런이 뽑혔고 List에서 제거됨

        RpcStartTurn(drawnID);
    }

    [ClientRpc]
    void RpcStartTurn(int drawnID)
    {
        Hand.AddHand(Deck.Villains[drawnID]); //모든 클라이언트의 리모트 객체의 Hand에 해당 ID를 가진 Villain이 추가
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

        //차례를 넘길 수 있는 버튼이 활성화 됨
    }

    [Command]
    public void EndTurn()
    {
        if (hand.IsGameOver)
        {
            GameManager.instance.GameOver(myOrder);
            return;
        }

        print($"{myOrder + 1}번째 플레이어가 차례를 마침");

        // 턴 종료 UI효과
        // 모든 플레이어 화면이동...

        if (isLocalPlayer)
            GameManager.instance.NextTurn(myOrder);
    }
}
