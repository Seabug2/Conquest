using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public interface ICardState
{
    public void OnPointerClick(PointerEventData eventData);
    public void OnPointerEnter(PointerEventData eventData);
    public void OnPointerEixt(PointerEventData eventData);
    public void OnPointerDown(PointerEventData eventData);
    public void OnDrag(PointerEventData eventData);
    public void OnPointerUp(PointerEventData eventData);
}

/// <summary>
///ī�� ���� ���콺�� �÷��� �ƹ��� ������ ���� �ʴ� ����
/// </summary>
public class None : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}

public class InDeck : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }

}

/// <summary>
/// ī�带 �ʵ忡 �� ���
/// </summary>
public class OnField : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}

//������ ī�带 ���� ��...
public class OnDraftZone : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}

//������ ī�带 ���� ��...
public class OnSelect : ICardState
{
    Card card;

    public OnSelect(Action _OnClieckEvent)
    {
        ClickEvent = _OnClieckEvent;
        // ī�带 Ŭ������ ��,
        // 1. UICanvas�� Ȱ��ȭ���� �ٸ� ī�带 ������ �� ���� ���´�.
        // 2. ��ȭâ UI�� Yse ��ư��
    }

    public Action ClickEvent;

    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    
    public void OnPointerClick(PointerEventData eventData) 
    {
        //��ȭâ UI�� Ȱ��ȭ ��Ų��.
        //��ȭâ UI�� Yse ��ư�� ClickEvent�� ���...
        //ClickEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}