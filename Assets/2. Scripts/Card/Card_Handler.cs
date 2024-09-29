using System;
using UnityEngine.EventSystems;
using UnityEngine;
using Mirror;

public partial class Card : NetworkBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ICardState iCardState;

    //카드 드래그를 시작했을 때 호출됩니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
        iCardState.OnPointerDown(eventData);
    }

    //카드를 드래그하는 중에 지속적으로 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }

    //카드 드래그를 마쳤을 때 호출됩니다.
    public void OnEndDrag(PointerEventData eventData)
    {
        iCardState.OnPointerUp(eventData);
    }






    public Action<Card> OnPointerCardEnter;
    //카드 위에 마우스를 올려두었을 때 한 번만 호출 됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsOpened)
        {
            //UIManager.instance.info.PopUp(front);
            OnPointerCardEnter?.Invoke(this);
        }
        iCardState.OnPointerEnter(eventData);
    }

    //마우스가 카드 밖으로 이동할 때 한 번만 호출 됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        iCardState.OnPointerEixt(eventData);
    }







    //카드를 마우스로 클릭 할 때마다 호출 됩니다.
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
