using UnityEngine.EventSystems;

public interface ICardState
{
    public void OnPointerClick(PointerEventData eventData);
    public void OnPointerEnter(PointerEventData eventData);
    public void OnPointerEixt(PointerEventData eventData);
    public void OnPointerDown(PointerEventData eventData);
    public void OnDrag(PointerEventData eventData);
    public void OnPointerUp(PointerEventData eventData);
}

/// <summary>
///카드 위에 마우스를 올려도 아무런 반응을 하지 않는 상태
/// </summary>
public class NoneState : ICardState
{
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnPointerUp(PointerEventData eventData) { }
    public void OnPointerClick(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerEixt(PointerEventData eventData) { }
}