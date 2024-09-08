using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    [SerializeField, Header("�̹����� Ȱ��ȭ�� ���·� �����մϴ�.")]
    bool startWithBlack;

    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        img.enabled = startWithBlack;
    }

    Tween tween;

    #region ���̵� ��
    public void In(float duration = 1)
    {
        img.color = Color.black;
        img.enabled = true;
        tween = img.DOFade(0, duration)
            .SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                img.enabled = false;
            });
    }

    public void In(float duration = 1, Action onFadeIn = null)
    {
        img.color = Color.black;
        img.enabled = true;
        tween = img.DOFade(0, duration)
            .SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                img.enabled = false;
                onFadeIn?.Invoke();
            });
    }
    #endregion

    #region ���̵� �ƿ�
    public void Out(float duration = 1)
    {
        img.color = new Color(1, 1, 1, 0);  // �̹����� �ʱ� ���� ����
        img.enabled = true;

        tween = img.DOFade(1, duration)
            .SetEase(Ease.InQuint);
    }

    public void Out(float duration = 1, Action onFadeOut = null)
    {
        img.color = new Color(1, 1, 1, 0);  // �̹����� �ʱ� ���� ����
        img.enabled = true;

        tween = img.DOFade(1, duration)
            .SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                onFadeOut?.Invoke();
            });
    }
    #endregion
}
