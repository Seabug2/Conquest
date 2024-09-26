using System;
using UnityEngine;
using UnityEngine.EventSystems;

//덱에서 카드를 꺼낼 때...
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
        //TODO 선택 가능한 상태의 카드가 마우스에 반응하는 효과를 추가
        card.transform.localScale = Vector3.one * 1.35f;
    }

    public void OnPointerEixt(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one;
    }
}