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

    //NetworkPlayer.EndTurn���� ȣ��Ǿ� �� ������ �ʰ��ߴ��� Ȯ���մϴ�.
    public bool IsGameOver() => list.Count > HandsLimit();

    public void AddHand(Card drawnCard)
    {
        list.Add(drawnCard);

        UpdateHand();
        //ī�带...
    }

    readonly float handWidthHalf = 2f;

    public void UpdateHand()
    {
        //ī�尡 �п� �߰� �Ǿ��� ��,
        //ī�尡 �и� ����� �� (������, �ʵ��, �ٸ� ����� �з� �̵�)
        int handCount = list.Count;

        if (handCount > 3)
        {
            //ī�� ���� 4�� �̻��� ��� �а� ȣ�� �׸��� ��ġ��
        }
        else
        {
            //ī�� ���� 3�� ���϶�� �ݵ��ϰ� ��ġ��
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
