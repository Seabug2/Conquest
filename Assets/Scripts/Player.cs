using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    //�ڽ��� �տ� ��� �ִ� ī��
    public List<Card> hands = new List<Card>();
    //�ڽ��� �տ� �� ī��
    public List<Card> front = new List<Card>();

    public int handLimitStack = 0;

    public bool IsGameOver { get; private set; }

    private void Start()
    {
        IsGameOver = false;
    }

    public bool IsHandLimitOver()
    {
        //�ڽ��� ���ʸ� ��ĥ�� �÷��̾��� �а� 6�庸�� ���ٸ� �й��Ѵ�.
        return hands.Count <= (6 + handLimitStack);
    }
}
