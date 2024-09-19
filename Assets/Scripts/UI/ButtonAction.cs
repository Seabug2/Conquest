using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonAction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    Image img;
    Color onMouseColor = Color.white;
    Color offMouseColor = new Color(1, 1, 1, 0.8f);

    /// <summary>
    /// 원하는 상호작용 Action을 등록
    /// 그리고 대화창을 비활성화 시키는 코드를 등록
    /// </summary>
    public Action OnClickEvent;

    private void Start()
    {
        img = GetComponent<Image>();
        img.color = offMouseColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEvent?.Invoke();
        img.color = offMouseColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        img.color = onMouseColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        img.color = offMouseColor;
    }
}
