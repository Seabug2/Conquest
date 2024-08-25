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
        //호스트에서만 카드를 리필
        if (isServer)
            for (int i = 0; i < 60; i++)
            {
                deck.Add(i);
            }
    }

    [SerializeField] Transform[] loungePosition;

    /// <summary>
    /// 플레이어의 수 만큼 덱에서 카드를 공개
    /// </summary>
    public void Open()
    {


    }
}
