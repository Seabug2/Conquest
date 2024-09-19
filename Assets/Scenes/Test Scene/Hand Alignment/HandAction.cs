using UnityEngine;

[RequireComponent(typeof(Hand))]
public class HandAction : MonoBehaviour
{
    public Card prefab;
    Hand hand;

    private void Start()
    {
        hand = GetComponent<Hand>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hand.Add(Instantiate(prefab));
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            hand.HandOpen();
        }
    }
}