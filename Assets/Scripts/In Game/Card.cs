using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(CardHandler))]
public class Card : MonoBehaviour
{
    [SerializeField] int id;
    public int ID => id;

    [SerializeField] string cardName;
    public string CardName => cardName;

    [SerializeField] string favorText;
    public string FavorText => favorText;

    [SerializeField] string description;
    public string Description => description;

    [SerializeField]
    NetworkPlayer owner;
    public NetworkPlayer Owner => owner;

    /// <summary>
    /// 카드를 전개 했을 때, 혹은 효과를 발동할 때 나올 컷씬
    /// </summary>
    [SerializeField] GameObject eventScene;

    //소켓에 대한 정보
    // 0   1
    //
    // 3   2
    [SerializeField] Socket[] sockets;
    public Socket[] Sockets => sockets;

    public CardHandler handler;

    private void Awake()
    {
        handler = GetComponent<CardHandler>();
    }
}
