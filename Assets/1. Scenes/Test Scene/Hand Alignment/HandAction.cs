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

    [Server]
    void AddCard()
    {
        GameObject obj = Instantiate(prefab).gameObject;
        NetworkServer.Spawn(obj);
        hand.TestAdd(obj.GetComponent<Card>());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddCard();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            hand.TestChangeState();
        }
        if (Input.GetKeyDown(KeyCode.A))
            hand.HandAlignment();
    }
}