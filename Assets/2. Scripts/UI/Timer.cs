using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Timer : MonoBehaviour, IUIController
{
    [SerializeField] GameObject root;
    [SerializeField] RectTransform gage;
    [SerializeField] Text counter;

    bool isPlaying = false;
    public bool IsPlaying() => isPlaying;

    Action OnTimeOutEvent;
    LineMessage message;
    LineMessage Message
    {
        get
        {
            if(message == null)
            {
                message = UIManager.GetUI<LineMessage>();
            }
            return message;
        }
    }

    private void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }

        root.SetActive(false);
        gage.localScale = new Vector3(0, 1, 1);

        counter.text = string.Empty;
        counter.gameObject.SetActive(false);

        OnTimeOutEvent = null;
        isPlaying = false;
    }

    /// <summary>
    /// Timer È°¼ºÈ­
    /// </summary>
    public void Active(float duration = 1f)
    {
        if (root.activeSelf) return;

        root.gameObject.SetActive(true);
        gage.DOScaleX(1, duration).SetEase(Ease.OutQuad);
    }

    public void Pause()
    {
        isPlaying = false;
    }

    
    public void Resume()
    {
        isPlaying = true;
    }



    public void Stop()
    {
        isPlaying = false;

        if (counter.gameObject.activeSelf)
        {
            counter.DOKill();
            counter.gameObject.SetActive(false);
        }
    }

    public void @Reset()
    {
        isPlaying = false;
        OnTimeOutEvent = null;
        if (counter.gameObject.activeSelf)
        {
            counter.DOKill();
            counter.gameObject.SetActive(false);
        }
        gage.DOKill();
        gage.DOScaleX(1, 1f).SetEase(Ease.InQuad);
    }

    public void Play(float maxValue, Action OnTimeOutEvent = null)
    {
        this.OnTimeOutEvent = OnTimeOutEvent;

        isPlaying = true;
        this.maxValue = maxValue;
        t = maxValue;

        gage.DOKill();
    }

    public void Inactive(float duration = 1f)
    {
        isPlaying = false;
        OnTimeOutEvent = null;
        gage.DOKill();
        gage
            .DOScaleX(0, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => root.gameObject.SetActive(false));
    }

    float t = 0;
    float maxValue = 1;

    const int minCount = 5;

    private void Update()
    {
        if (!isPlaying || Message.IsPlaying()) return;

        t -= Time.deltaTime;

        float value = Mathf.Clamp01(t / maxValue);

        int i = Mathf.FloorToInt(t);
        if (i <= minCount)
        {
            if (!counter.gameObject.activeSelf)
            {
                counter.gameObject.SetActive(true);
                counter.rectTransform.DOPunchAnchorPos(new Vector2(5f, 5f), 1f).SetLoops(minCount + 1);
            }

            counter.text = i.ToString();
        }

        gage.localScale = new Vector3(value, 1, 1);

        if (t <= 0)
        {
            isPlaying = false;

            OnTimeOutEvent?.Invoke();
            
            if (counter.gameObject.activeSelf)
            {
                counter.rectTransform.DOKill();
                counter.gameObject.SetActive(false);
            }
        }
    }
}
