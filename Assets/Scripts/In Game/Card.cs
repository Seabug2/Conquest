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
    /// ī�带 ���� ���� ��, Ȥ�� ȿ���� �ߵ��� �� ���� �ƾ�
    /// </summary>
    [SerializeField] GameObject eventScene;

    //���Ͽ� ���� ����
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
