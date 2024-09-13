using UnityEngine.EventSystems;
using UnityEngine;
using Mirror;

public partial class Card : NetworkBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ICardState iCardState;

    public void SetState(ICardState iCardState)
    {
        this.iCardState = iCardState;
    }

    public bool IsOnMouse { get; private set; }


    public void OnBeginDrag(PointerEventData eventData)
    {
        iCardState.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        iCardState.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        iCardState.OnEndDrag(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsOpened)
        {
            UIMaster.InfoUI.PopUp(front);
        }
        
        iCardState.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        iCardState.OnPointerEixt(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        iCardState.OnPointerClick(eventData);
    }
}
