using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class GameManager : NetworkBehaviour
{
    Card[] cards = new Card[60];
    public SyncList<int> deck;

    private void Start()
    {
        //ȣ��Ʈ������ ī�带 ����
        if (isServer)
            for (int i = 0; i < 60; i++)
            {
                deck.Add(i);
            }
    }

    [SerializeField] Transform[] loungePosition;

    /// <summary>
    /// �÷��̾��� �� ��ŭ ������ ī�带 ����
    /// </summary>
    public void Open()
    {


    }
}
