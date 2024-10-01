using System;
using UnityEngine.EventSystems;

public partial class Card : 
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ICardState iCardState;

    //ī�带 �巡���ϴ� �߿� ���������� ȣ��˴ϴ�.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }





    public Action<Card> OnPointerCardEnter;
    //ī�� ���� ���콺�� �÷��ξ��� �� �� ���� ȣ�� �˴ϴ�.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsOpened)
        {
            //UIManager.instance.info.PopUp(front);
            OnPointerCardEnter?.Invoke(this);
        }
        iCardState.OnPointerEnter(eventData);
    }

    public Action<Card> OnPointerCardExit;
    //���콺�� ī�� ������ �̵��� �� �� ���� ȣ�� �˴ϴ�.
    public void OnPointerExit(PointerEventData eventData)
    {
        iCardState.OnPointerEixt(eventData);
    }







    //ī�带 ���콺�� Ŭ�� �� ������ ȣ�� �˴ϴ�.
    public void OnPointerClick(PointerEventData eventData)
    {
        iCardState.OnPointerClick(eventData);
    }

    public Action<Card> OnPointerCardDown;
    public void OnPointerDown(PointerEventData eventData)
    {
        iCardState.OnPointerDown(eventData);
    }

    public Action<Card> OnPointerCardUp;
    public void OnPointerUp(PointerEventData eventData)
    {
        iCardState.OnPointerUp(eventData);
    }
}
