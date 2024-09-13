using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LineMessage : MonoBehaviour
{
    [SerializeField] RectTransform line;
    [SerializeField] Text text;

    const float height = 200f;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ForcePopUp(string message, float duration)
    {
      
    }

    public void PopUp(string message, float duration)
    {
        
    }
}
