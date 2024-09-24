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


}
