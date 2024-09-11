using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    [SerializeField, Header("�̹����� Ȱ��ȭ�� ���·� �����մϴ�.")]
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

    #region ���̵� ��
    public void In(float duration = 1, Action onFadeIn = null)
    {
        StartFade(Color.black, 0, duration, onFadeIn);
    }
    #endregion

    #region ���̵� �ƿ�
    public void Out(float duration = 1, Action onFadeOut = null)
    {
        StartFade(new Color(1, 1, 1, 0), 1, duration, onFadeOut);
    }
    #endregion

    private void StartFade(Color startColor, float targetAlpha, float duration, Action onComplete)
    {
        if (tween != null)
        {
            tween.Kill(); // ���� Ʈ���� �����Ͽ� �浹 ����
        }

        img.color = startColor;
        img.enabled = true;

        tween = img.DOFade(targetAlpha, duration)
            .SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                if (targetAlpha == 0)
                {
                    img.enabled = false; // ���̵� ���� ������ �� ��Ȱ��ȭ
                }
                onComplete?.Invoke();
            });
    }
}
