using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public partial class Card : NetworkBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ICardState iCardState;

    //ī�� �巡�׸� �������� �� ȣ��˴ϴ�.
    public void OnBeginDrag(PointerEventData eventData)
    {
        iCardState.OnBeginDrag(eventData);
    }

    //ī�带 �巡���ϴ� �߿� ���������� ȣ��˴ϴ�.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }

    //ī�� �巡�׸� ������ �� ȣ��˴ϴ�.
    public void OnEndDrag(PointerEventData eventData)
    {
        iCardState.OnEndDrag(eventData);
    }







    //ī�� ���� ���콺�� �÷��ξ��� �� �� ���� ȣ�� �˴ϴ�.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsOpened)
        {
            UIMaster.InfoUI.PopUp(front);
        }
        IsOnMouse = true;
        iCardState.OnPointerEnter(eventData);
    }

    //���콺�� ī�� ������ �̵��� �� �� ���� ȣ�� �˴ϴ�.
    public void OnPointerExit(PointerEventData eventData)
    {
        IsOnMouse = false;
        iCardState.OnPointerEixt(eventData);
    }







    //ī�带 ���콺�� Ŭ�� �� ������ ȣ�� �˴ϴ�.
    public void OnPointerClick(PointerEventData eventData)
    {
        iCardState.OnPointerClick(eventData);
    }
}
