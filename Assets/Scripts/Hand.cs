using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    List<Token> list = new List<Token>();

    const int handsLimit = 6;
    public int LimitStack { get; private set; }
    public void SetLimitStack(int i)
    {
        LimitStack += i;
    }
    public int HandsLimit { get { return handsLimit + LimitStack; } }

    public UnityEvent GameOverEvent;

    private void Start()
    {
        LimitStack = 0;
    }

    public void CloseTurn()
    {
        if (IsGameOver)
        {
            GameOverEvent.Invoke();
        }
    }

    public bool IsGameOver
    {
        get
        {
            //�ڽ��� ���ʸ� ��ĥ�� �÷��̾��� �а� ���� �� ���� ���ٸ� �й�
            if (list.Count > (HandsLimit))
            {
                return true;
            }
            else return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 1));
    }
}
