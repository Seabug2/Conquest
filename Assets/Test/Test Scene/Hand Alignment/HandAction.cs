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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Card card = Instantiate(prefab);
            NetworkServer.Spawn(card.gameObject);
            card.OnPointerCardEnter = (card) => UIManager.GetUI<Info>()?.PopUp(card);
            card.iCardState = new NoneState();
            hand.Add(card);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            hand.RandomDiscard();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hand.StateToggle();
        }
    }
}