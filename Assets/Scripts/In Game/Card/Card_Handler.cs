using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public partial class Card : NetworkBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ICardState iCardState;

    //카드 드래그를 시작했을 때 호출됩니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
        iCardState.OnBeginDrag(eventData);
    }

    //카드를 드래그하는 중에 지속적으로 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }

    //카드 드래그를 마쳤을 때 호출됩니다.
    public void OnEndDrag(PointerEventData eventData)
    {
        iCardState.OnEndDrag(eventData);
    }







    //카드 위에 마우스를 올려두었을 때 한 번만 호출 됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsOpened)
        {
            UIMaster.InfoUI.PopUp(front);
        }
        IsOnMouse = true;
        iCardState.OnPointerEnter(eventData);
    }

    //마우스가 카드 밖으로 이동할 때 한 번만 호출 됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        IsOnMouse = false;
        iCardState.OnPointerEixt(eventData);
    }







    //카드를 마우스로 클릭 할 때마다 호출 됩니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        iCardState.OnPointerClick(eventData);
    }
}
