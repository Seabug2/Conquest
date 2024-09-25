using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HeadLine : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] RectTransform root;

    private void Start()
    {
        text.text = string.Empty;
        root.sizeDelta.Set(0, root.sizeDelta.y);
    }

    public void On()
    {
        root.DOSizeDelta(new Vector2( Screen.width, root.sizeDelta.y),1f).SetEase(Ease.OutQuart);
    }

    public void Set(string _text)
    {
        text.text = string.Empty;
        text.DOText(_text, 1f);
    }
}
