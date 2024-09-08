using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    [SerializeField, Header("이미지를 활성화한 상태로 시작합니다.")]
    bool startWithBlack;

    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        img.enabled = startWithBlack;
    }

    Tween tween;

    #region 페이드 인
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

    #region 페이드 아웃
    public void Out(float duration = 1)
    {
        img.color = new Color(1, 1, 1, 0);  // 이미지의 초기 상태 설정
        img.enabled = true;

        tween = img.DOFade(1, duration)
            .SetEase(Ease.InQuint);
    }

    public void Out(float duration = 1, Action onFadeOut = null)
    {
        img.color = new Color(1, 1, 1, 0);  // 이미지의 초기 상태 설정
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
