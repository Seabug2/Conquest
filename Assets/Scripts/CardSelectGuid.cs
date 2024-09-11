using System;
using UnityEngine;

public class CardSelectGuid : MonoBehaviour
{
    public Card connectCard;

    public Action<Card> OnClickAction;

    private void OnMouseEnter()
    {
        
    }
    private void OnMouseExit()
    {
        
    }

    private void OnMouseDown()
    {
        OnClickAction?.Invoke(connectCard);
        gameObject.SetActive(false);
    }
}
