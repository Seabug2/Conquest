using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LineMessage : MonoBehaviour
{
    [SerializeField] GameObject cull;
    [SerializeField] RectTransform line;
    [SerializeField] Text text;

    Sequence sequence;

    const float height = 200f;

    private void Start()
    {
        cull.SetActive(false);
    }

    public void ForcePopUp(string message, float duration)
    {
        if (sequence == null)
            sequence.Kill();

        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                text.text = message;
                line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
                cull.SetActive(true);
            })
            .SetAutoKill(true) //����� �������� �����Ѵ�.
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, height), duration * 0.1f).SetEase(Ease.OutQuint))
            .AppendInterval(duration * 0.8f)
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, 0), duration * 0.1f).SetEase(Ease.OutQuint));

        sequence.Play().OnComplete(() =>
        {
            cull.SetActive(false);
        });
    }

    public void PopUp(string message, float duration)
    {
        //�������� ���� ���̶�� �۵����� ����
        if (sequence != null && sequence.IsPlaying()) return;

        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                text.text = message;
                line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
                cull.SetActive(true);
            })
            .SetAutoKill(true) //����� �������� �����Ѵ�.
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, height), duration * 0.1f).SetEase(Ease.OutQuint))
            .AppendInterval(duration * 0.8f)
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, 0), duration * 0.1f).SetEase(Ease.OutQuint));

        sequence.Play().OnComplete(() =>
        {
            cull.SetActive(false);
        });
    }

    public void PopUp(string message, float duration, Action OnCloseAction = null)
    {
        //�������� ���� ���̶�� �۵����� ����
        if (sequence != null && sequence.IsPlaying()) return;

        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                text.text = message;
                line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
                cull.SetActive(true);
            })
            .SetAutoKill(true) //����� �������� �����Ѵ�.
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, height), duration * 0.1f).SetEase(Ease.OutQuint))
            .AppendInterval(duration * 0.8f)
            .Append(DOTween.To(() => line.sizeDelta, x => line.sizeDelta = x, new Vector2(line.sizeDelta.x, 0), duration * 0.1f).SetEase(Ease.OutQuint));

        sequence.Play().OnComplete(() =>
        {
            OnCloseAction?.Invoke();
            cull.SetActive(false);
        });
    }
}
