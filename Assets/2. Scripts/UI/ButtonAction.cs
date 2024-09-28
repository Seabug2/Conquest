using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    Image img;
    //Color onMouseColor = Color.white;
    Color offMouseColor = new Color(1, 1, 1, 0.8f);

    // '=' ������ ����Ͽ� �޼��带 ����ϱ����� event�� ������� �ʴ´�.
    public Action OnClickEvent;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void Start()
    {
        img.color = offMouseColor;
    }

    private void OnEnable()
    {
        img.color = offMouseColor;
    }

    private void OnDisable()
    {
        img.color = offMouseColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEvent?.Invoke();
        img.color = offMouseColor;
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        img.color = Color.white;
        transform.localScale = Vector3.one * 1.3f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        img.color = offMouseColor;
        transform.localScale = Vector3.one;
    }
}
