using System;
using UnityEngine.EventSystems;

public partial class Card : 
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public ICardState iCardState;

    //카드를 드래그하는 중에 지속적으로 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
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

    public Action<Card> OnPointerCardExit;
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
