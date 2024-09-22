using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class InHandState : ICardState
{
    readonly Card card;
    readonly Hand hand;

    public InHandState(Card card, Hand hand)
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
        card.SprtRend.sortingLayerName = "OnMouseLayer";
        card.transform.localScale = Vector3.one * 1.2f;
        card.IsOnMouse = true;
        card.Pick();
        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;
        card.IsOnMouse = false;
        hand.HandAlignment();
    }
}


/// <summary>
/// �ڽ��� ���ʿ� ī�带 ���� ����
/// </summary>
public class HandlingState : ICardState
{
    readonly Card card;
    readonly Hand hand;
    readonly Field field;

    public HandlingState(Card card, Hand hand, Field field)
    {
        this.card = card;
        this.hand = hand;
        this.field = field;
    }

    bool isSelected = false;
    private Vector3 offset;
    float z;

    public void OnPointerDown(PointerEventData eventData)
    {
        Camera.main.GetComponent<Physics2DRaycaster>().eventMask = 0;

        isSelected = true;

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
        isSelected = false;

        Camera.main.GetComponent<Physics2DRaycaster>().eventMask = -1;

        Vector2 screenPosition = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, 1 << LayerMask.NameToLayer("Tile"));

        if (hit.collider == null)
        {
            // ����ĳ��Ʈ�� �ƹ��͵� �������� ���� ���
            card.DoMove();
            Debug.Log("No object hit in 2D.");
        }
        else
        {
            // ����ĳ��Ʈ�� ������Ʈ�� ������ ���
            if (hit.collider.TryGetComponent<Tile>(out Tile tile))
            {
                if (tile.IsSetable(card))
                {
                    Debug.Log("Hit 2D object: " + hit.collider.gameObject.name);
                    hand.CmdRemove(card.ID);
                    tile.CmdSetCard(card.ID);

                    card.iCardState = new NoneState();
                }
                else
                {
                    Debug.LogError("�� Ÿ�Ͽ��� ī�带 �� �� �����ϴ�.");
                }

            }
            else
            {
                Debug.LogError("��Ʈ�� ������Ʈ�� Tile ������Ʈ�� �����ϴ�.");
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData)
    {
        card.IsOnMouse = true;
        if (isSelected) return;

        card.transform.localScale = Vector3.one * 1.2f;
        card.SprtRend.sortingLayerName = "OnMouseLayer";

        if (field != null)
            field.ShowPlaceableTiles(card, true);

        card.CmdPick();
        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        if (isSelected) return;
        card.IsOnMouse = false;

        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;

        if (field != null)
            field.ShowPlaceableTiles(card, false);

        card.CmdDoMove();
        hand.HandAlignment();
    }
}