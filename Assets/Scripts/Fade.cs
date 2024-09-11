using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    [SerializeField, Header("이미지를 활성화한 상태로 시작합니다.")]
    bool startWithBlack;

    Image img;
    Tween tween;

    private void Start()
    {
        img = GetComponent<Image>();
        img.enabled = startWithBlack;

        if (startWithBlack)
        {
            img.color = Color.black;
        }
        else
        {
            img.color = new Color(1, 1, 1, 0);
        }
    }

    #region 페이드 인
    public void In(float duration = 1, Action onFadeIn = null)
    {
        StartFade(Color.black, 0, duration, onFadeIn);
    }
    #endregion

    #region 페이드 아웃
    public void Out(float duration = 1, Action onFadeOut = null)
    {
        StartFade(new Color(1, 1, 1, 0), 1, duration, onFadeOut);
    }
    #endregion

    private void StartFade(Color startColor, float targetAlpha, float duration, Action onComplete)
    {
        if (tween != null)
        {
            tween.Kill(); // 기존 트윈을 중지하여 충돌 방지
        }

        img.color = startColor;
        img.enabled = true;

        tween = img.DOFade(targetAlpha, duration)
            .SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                if (targetAlpha == 0)
                {
                    img.enabled = false; // 페이드 인이 끝났을 때 비활성화
                }
                onComplete?.Invoke();
            });
    }
}
