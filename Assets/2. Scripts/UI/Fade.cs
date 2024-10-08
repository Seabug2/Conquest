using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour, IUIController
{
    [SerializeField, Header("�⺻ ����")]
    Color defualtColor = Color.black;

    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.color = defualtColor;
        image.enabled = true;
    }

    private void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }
    }

    public void In(float duration = 1, Action OnComplete = null)
    {
        if (!gameObject.activeSelf) return;
        isPlaying = true;
        image.DOKill();

        // 1 => 0
        image.DOFade(0, duration)
            .OnComplete(() =>
            {
                OnComplete?.Invoke();
                isPlaying = false;
                gameObject.SetActive(false);
            });
    }
 
    public void Out(float duration = 1, float r = 0, float g = 0, float b = 0, Action OnComplete = null)
    {
        image.DOKill();
        image.color = new Color(r, g, b, 0);
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        isPlaying = true;
        // 0 => 1
        image.DOFade(1, duration)
            .OnComplete(() =>
            {
                OnComplete?.Invoke();
                isPlaying = false;
            });
    }

    bool isPlaying = false;

    public bool IsPlaying() => isPlaying;
}
