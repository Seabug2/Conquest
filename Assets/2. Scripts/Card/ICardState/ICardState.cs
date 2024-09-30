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
public class NoneState : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}