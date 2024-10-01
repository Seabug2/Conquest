using UnityEngine;
using Mirror;

[RequireComponent(typeof(Hand))]
public class HandAction : NetworkBehaviour
{
    public Card prefab;
    Hand hand;

    private void Start()
    {
        hand = GetComponent<Hand>();
    }
}