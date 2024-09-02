using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//플레이어 객체
public class Player : NetworkBehaviour
{
    [SyncVar] //Hook?
    public int myOrder = 0;

    Hand hand;
    Field field;

    private void Awake()
    {
        hand = GetComponent<Hand>();
    }

    private void Start()
    {
        if (isServer)
            //게임매니저를 찾아서
            FindObjectOfType<GameManager>().AddPlayer(this);
    }

    [ClientRpc]
    public void SetUp(int i)
    {
        myOrder = i;
        if (!isLocalPlayer) return;

        //자신의 카메라 찾기
        Camera.main.GetComponent<CameraController>().Init(i);

        //자신의 필드 찾기

        //자신의 핸드 찾기

    }

    [ClientRpc]
    public void StartTurn()
    {
        //자신의 차례를 시작
        //모든 클라이언트에서 플레이어 객체에서 실행되어야 할 것들...
        // 1) 차례를 알리는 UI Pop Up

        // 2) UI가 사라진 후 로컬 플레이어에서만 드로우 UI가 팝업
        if (!isLocalPlayer) return;

        //로컬 플레이어는 덱에서 카드를 드로우할 수 있도록
        //드로우 UI가 나타난다.
    }
}
