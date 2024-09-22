using System;
using UnityEngine;
using UnityEngine.EventSystems;

//µ¦¿¡¼­ Ä«µå¸¦ ²¨³¾ ¶§...
public class SelectionState : ICardState
{
    readonly Card card;
    event Action OnClieckEvent;

    public SelectionState(Card _card, Action _OnClieckEvent)
    {
        card = _card;
        OnClieckEvent = _OnClieckEvent;
    }

    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one;
        OnClieckEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one * 1.35f;
    }

    public void OnPointerEixt(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one;
    }
}