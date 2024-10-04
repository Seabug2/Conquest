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

    #region 기본 상태에서는 드래그 이벤트가 없다.
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
        card.Pick(hand.selectedCardHeight.position.y);
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
/// 자신의 차례에 카드를 내는 순간
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

    /*
     * 드래그를 할 때
     * 카드가 마우스의 움직임을 따라가지 못하면서
     * 순간적으로 Exit 이벤트가 발생,
     * 드래그를 마칠 때까지 Enter&Eixt 이벤트의 발생을 막기 위해
     * bool값 트리거를 사용
     */
    bool isSelected = false;
    private Vector3 offset;
    float z;

    public void OnPointerDown(PointerEventData eventData)
    {
        //카메라컨트롤러에서 이벤트 레이어를 비활성화하고
        //화면이동을 막는다.
        card.OnPointerCardDown?.Invoke(card);

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
        card.OnPointerCardUp?.Invoke(card);

        isSelected = false;

        Vector2 screenPosition = eventData.position;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, 1 << LayerMask.NameToLayer("Tile"));

        if (hit.collider == null)
        {
            // 레이캐스트가 아무것도 감지하지 못한 경우
            //card.DoMove(()=>card.col.enabled = true);
            Debug.Log("No object hit in 2D.");
            card.DoMove();
        }
        else
        {
            // 레이캐스트가 오브젝트를 감지한 경우
            if (hit.collider.TryGetComponent<Tile>(out Tile tile))
            {
                if (tile.IsSetable(card))
                {
                    card.iCardState = GameManager.instance.noneState;
                    Debug.Log("Hit 2D object: " + hit.collider.gameObject.name);
                    hand.CmdRemove(card.id);
                    tile.CmdSetCard(card.id);
                }
                else
                {
                    UIManager.GetUI<LineMessage>().PopUp("그 타일에는 둘 수 없습니다.", 1.6f);
                    card.IsOnMouse = false;
                    hand.HandAlignment();
                }
            }
        }

        field.ShowPlaceableTiles(null, false);
    }

    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;
        card.IsOnMouse = true;

        card.transform.localScale = Vector3.one * 1.2f;
        card.SprtRend.sortingLayerName = "OnMouseLayer";

        field?.ShowPlaceableTiles(card, true);

        card.CmdPick(hand.selectedCardHeight.position.y);
        hand.HandAlignment();
    }
    public void OnPointerEixt(PointerEventData eventData)
    {
        if (isSelected) return;
        card.IsOnMouse = false;

        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;

        field?.ShowPlaceableTiles(card, false);

        card.CmdDoMove();
        hand.HandAlignment();
    }
}