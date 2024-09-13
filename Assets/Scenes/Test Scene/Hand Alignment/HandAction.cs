using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hand))]
public class HandAction : MonoBehaviour
{
    Hand hand;
    
    public Card prefab;

    private void Start()
    {
        hand = GetComponent<Hand>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hand.Add(Instantiate(prefab));
            hand.HandAlignment();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {

        }
    }
}
