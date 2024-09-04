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

    Sequence sequence;
    //Tween tween;

    public void MessagePopUp(string message, float duration)
    {
        text.text = message;
        line.sizeDelta = new Vector2(1, 0);
        sequence = DOTween.Sequence()
            .SetAutoKill(true).Pause()
            .Prepend(line.DOScaleY(0, 0))
            .Append(line.DOScaleY(1, duration * 0.1f).SetEase(Ease.OutQuint))
            .AppendInterval(duration * 0.8f)
            .Append(line.DOScaleY(0, duration * 0.1f).SetEase(Ease.InQuint));
        sequence.Play();
    }
}
