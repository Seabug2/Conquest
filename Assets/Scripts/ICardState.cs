using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public interface ICardState
{
    public void Enter();
    public void Exit();
    public void OnPointerClick(PointerEventData eventData);
    public void OnPointerEnter(PointerEventData eventData);
    public void OnPointerEixt(PointerEventData eventData);
    public void OnBeginDrag(PointerEventData eventData);
    public void OnDrag(PointerEventData eventData);
    public void OnEndDrag(PointerEventData eventData);
}

/// <summary>
///카드 위에 마우스를 올려도 아무런 반응을 하지 않는 상태
/// </summary>
public class None : ICardState
{
    Card card;
    
    public None(Card card)
    {
        this.card = card;
    }

    public void Enter() { }
    public void Exit() { }
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
}

public class InMyHand : ICardState
{
    public Card card;
    public Field field;

    public InMyHand(Card card, Field field)
    {
        this.field = field;
        this.card = card;
    }
    public void Enter() { }
    public void Exit() { }
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
}

public class InDeck : ICardState
{
    Card card;

    public InDeck(Card card)
    {
        this.card = card;
        Enter();
    }

    public void Enter()
    {
        card.IsOpened = false;
    }
    public void Exit() { }
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
}

/// <summary>
/// 카드를 필드에 낸 경우
/// </summary>
public class OnField : ICardState
{
    Card card;

    public OnField(Card card)
    {
        this.card = card;
    }
    public void Enter() { }
    public void Exit() { }
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
}

//덱에서 카드를 꺼낼 때...
public class OnDraftZone : ICardState
{
    Card card;

    public event Action OnClickEvent;

    public OnDraftZone(Card card)
    {
        this.card = card;
    }

    public void Enter() { }
    public void Exit() { }
    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
}