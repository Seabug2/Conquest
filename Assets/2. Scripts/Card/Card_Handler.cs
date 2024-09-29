using System;
using UnityEngine.EventSystems;
using UnityEngine;
using Mirror;

public partial class Card : NetworkBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ICardState iCardState;

    //ī�� �巡�׸� �������� �� ȣ��˴ϴ�.
    public void OnBeginDrag(PointerEventData eventData)
    {
        iCardState.OnPointerDown(eventData);
    }

    //ī�带 �巡���ϴ� �߿� ���������� ȣ��˴ϴ�.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }

    //ī�� �巡�׸� ������ �� ȣ��˴ϴ�.
    public void OnEndDrag(PointerEventData eventData)
    {
        iCardState.OnPointerUp(eventData);
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


    public void OnPointerDown(PointerEventData eventData)
    {
        iCardState.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        iCardState.OnPointerUp(eventData);
    }
}
