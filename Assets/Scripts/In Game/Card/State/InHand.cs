using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;


//ī�带 � �÷��̾��� �п� �߰��� ��,
//�� �÷��̾��� Ŭ���̾�Ʈ������
//�� ī�带 InHand ���·� �ٲ۴�.

//�ڽ��� ȭ�鿡���� ���� ī�� ���� ���콺�� �÷��θ�
//�а� ���ĵȴ�...

/// <summary>
/// �ڽ��� �п� �ִ� ī�忡 ���콺�� �÷��θ� ī�尡 ���� �ڴ´�.
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

    #region �⺻ ���¿����� �巡�� �̺�Ʈ�� ����.
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
/// �ڽ��� ���ʿ� ī�带 ���� ����
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
        Vector2 screenPosition = eventData.position;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Tile"));
        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            tile.CmdSetCard(card.id);

            //Tile�� CMD �޼��带 ����, �Ű������� �ڽ��� ID��...
            Debug.Log("Hit 2D object: " + hit.collider.gameObject.name);
        }
        else
        {
            card.DoMove();
            Debug.Log("No object hit in 2D.");
        }
    }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData)
    {
        card.transform.localScale = Vector3.one * 1.2f;
        card.SprtRend.sortingLayerName = "OnMouseLayer";

        card.CmdPick();
        field.ActiveTile(card, true);
        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;
        card.CmdDoMove();
        field.ActiveTile(card, false);
        hand.HandAlignment();
    }
}