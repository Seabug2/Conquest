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
    /// ���ϴ� ��ȣ�ۿ� Action�� ���
    /// �׸��� ��ȭâ�� ��Ȱ��ȭ ��Ű�� �ڵ带 ���
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
