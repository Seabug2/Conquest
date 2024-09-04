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

    //NetworkPlayer.EndTurn���� ȣ��Ǿ� �� ������ �ʰ��ߴ��� Ȯ���մϴ�.
    public bool IsGameOver
    {
        get
        {
            //�ڽ��� ���ʸ� ��ĥ�� �÷��̾��� �а� ���� �� ���� ���ٸ� �й�
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
        //ī�带...
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 1));
    }
}
