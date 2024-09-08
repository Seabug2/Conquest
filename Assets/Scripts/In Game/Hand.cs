using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    public int seatNum;

    [SerializeField] List<Card> list = new List<Card>();

    const int handsLimit = 6;
    public int LimitStack { get; private set; }

    public void SetLimitStack(int i)
    {
        LimitStack += i;
    }

    public int HandsLimit() => handsLimit + LimitStack;

    private void Start()
    {
        LimitStack = 0;
    }

    //NetworkPlayer.EndTurn에서 호출되어 패 제한을 초과했는지 확인합니다.
    public bool IsGameOver() => list.Count > HandsLimit();

    public void AddHand(Card drawnCard)
    {
        list.Add(drawnCard);

        UpdateHand();
        //카드를...
    }

    readonly float handWidthHalf = 2f;

    public void UpdateHand()
    {
        //카드가 패에 추가 되었을 때,
        //카드가 패를 벗어났을 때 (덱으로, 필드로, 다른 사람의 패로 이동)
        int handCount = list.Count;

        if (handCount > 3)
        {
            //카드 수가 4장 이상일 경우 패가 호를 그리며 배치됨
        }
        else
        {
            //카드 수가 3장 이하라면 반듯하게 배치됨
            float offset = 1 / (handCount + 1);

            for (int i = 0; i < handCount; i++)
            {
                list[i].handler.SetPosition(Vector3.Lerp(transform.position - Vector3.right * handWidthHalf
                    , transform.position + Vector3.right * handWidthHalf
                    , offset + (i + 1)));

                //list[i].transform.rotation = Quaternion.identity;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 1));
    }
}
