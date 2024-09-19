using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


//카드를 어떤 플레이어의 패에 추가할 때,
//그 플레이어의 클라이언트에서만
//그 카드를 InHand 상태로 바꾼다.

//자신의 화면에서만 패의 카드 위에 마우스를 올려두면
//패가 정렬된다...

/// <summary>
/// 자신의 패에 있는 카드에 마우스를 올려두면 카드가 위로 솟는다.
/// </summary>
public class InHand : ICardState
{
    Card card;
    Hand hand;

    public InHand(Card card, Hand hand)
    {
        this.card = card;
        this.hand = hand;
    }

    #region 기본 상태에서는 드래그 이벤트가 없다.
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    #endregion

    public void OnPointerEnter(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one * 1.3f;
        card.SprtRend.sortingLayerName = "OnMouseLayer";

        card.Pick();

        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;
        hand.HandAlignment();
    }
}


/// <summary>
/// 자신의 차례에 카드를 내는 순간
/// </summary>
public class InHandOnTurn : ICardState
{
    Card card;
    Hand hand;
    Field field;

    public InHandOnTurn(Card card, Hand hand, Field field)
    {
        this.card = card;
        this.hand = hand;
        this.field = field;
    }

    private Vector3 offset;
    float z;

    public void OnPointerDown(PointerEventData eventData)
    {
        card.transform.DOKill();
        z = Camera.main.WorldToScreenPoint(card.transform.position).z;
        offset = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, z)) - card.transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        card.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, z)) - offset;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //손을 떼었을 때 마우스가 타일 위에 있을 때

        //손을 떼었을 때 마우스가 타일 위에 없을 때
        card.DoMove();
    }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one * 1.2f;
        card.SprtRend.sortingLayerName = "OnMouseLayer";

        card.CmdPick();
        field?.ActiveTile(card, true);
        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;
        card.CmdDoMove();
        field?.ActiveTile(card, false);
        hand.HandAlignment();
    }
}