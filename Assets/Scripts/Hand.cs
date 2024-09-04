using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    List<Villain> list = new List<Villain>();

    public int seatNum;

    const int handsLimit = 6;
    public int LimitStack { get; private set; }
    public void SetLimitStack(int i)
    {
        LimitStack += i;
    }
    public int HandsLimit { get { return handsLimit + LimitStack; } }

    private void Start()
    {
        LimitStack = 0;
    }

    //NetworkPlayer.EndTurn에서 호출되어 패 제한을 초과했는지 확인합니다.
    public bool IsGameOver
    {
        get
        {
            //자신의 차례를 마칠때 플레이어의 패가 제한 수 보다 많다면 패배
            if (list.Count > HandsLimit)
            {
                return true;
            }
            else return false;
        }
    }

    public void AddHand(Villain drawnVillain)
    {
        list.Add(drawnVillain);
        //카드를...
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 1));
    }
}
