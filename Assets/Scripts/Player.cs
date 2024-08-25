using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    //자신이 손에 들고 있는 카드
    public List<Card> hands = new List<Card>();
    //자신의 앞에 낸 카드
    public List<Card> front = new List<Card>();

    public int handLimitStack = 0;

    public bool IsGameOver { get; private set; }

    private void Start()
    {
        IsGameOver = false;
    }

    public bool IsHandLimitOver()
    {
        //자신의 차례를 마칠때 플레이어의 패가 6장보다 많다면 패배한다.
        return hands.Count <= (6 + handLimitStack);
    }
}
