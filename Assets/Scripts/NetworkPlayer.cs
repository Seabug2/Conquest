using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//플레이어 객체
public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetUp))] //Hook?
    public int myOrder;
    public Hand hand { get; private set; }
    public Field field { get; private set; }

    private void Start()
    {
        //Host의 씬에 플레이어 오브젝트가 추가될 때마다 GameManager의 Player Dictionary에 오브젝트를 추가한다.
        if (isServer)
            GameManager.instance.AddPlayer(this);
    }

    public void SetUp(int _, int value)
    {
        //????????????????????????????????????????????????????????????????????????화면에서 Waiting UI 비활성화 / 삭제

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

        if (isLocalPlayer)
            //자신의 카메라 찾기
            Camera.main.GetComponent<CameraController>().Init(value);

    }

    [ClientRpc]
    public void StartTurn()
    {
        print($"{myOrder + 1}번째 플레이어의 차례");
        //자신의 차례를 시작
        //모든 클라이언트에서 플레이어 객체에서 실행되어야 할 것들...
        // 1) 차례를 알리는 UI Pop Up
        // 카메라가 자신의 화면을 주목하도록 바꿈

        // 2) UI가 사라진 후 로컬 플레이어에서만 드로우 UI가 팝업
        if (!isLocalPlayer) return;

        //로컬 플레이어는 덱에서 카드를 드로우할 수 있도록
        //드로우 UI가 나타난다.
    }

    [ClientRpc]
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
