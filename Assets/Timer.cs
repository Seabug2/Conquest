using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Timer : MonoBehaviour, IUIController
{
    [SerializeField] GameObject root;
    [SerializeField] RectTransform gage;
    [SerializeField] Text counter;

    //bool isCounting = false;
    //public bool IsCounting => isCountingage.localScale.x > 0;
    public bool IsPlaying { get; private set; }

    private void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(this.GetType(), this);
        }
        root.gameObject.SetActive(false);
        gage.localScale = new Vector3(0, 1, 1);

        counter.text = "5";
        counter.gameObject.SetActive(false);

        IsPlaying = false;
    }

    public void On(float duration = 1f)
    {
        if (root.gameObject.activeSelf) return;

        root.gameObject.SetActive(true);
        gage.DOScaleX(1, duration).SetEase(Ease.OutQuad);
    }

    public void Stop()
    {
        IsPlaying = false;
        if (counter.gameObject.activeSelf)
        {
            counter.DOKill();
            counter.gameObject.SetActive(false);
        }
        gage.DOKill();
    }

    public void @Reset()
    {
        IsPlaying = false;
        if (counter.gameObject.activeSelf)
        {
            counter.DOKill();
            counter.gameObject.SetActive(false);
        }
        gage.DOKill();
        gage.DOScaleX(1, 1f).SetEase(Ease.InQuad);
    }

    public void SetTimer(float maxValue)
    {
        IsPlaying = true;
        this.maxValue = maxValue;
        t = maxValue;
    }

    public void Off(float duration = 1f)
    {
        IsPlaying = false;
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
        if (!IsPlaying) return;

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
            IsPlaying = false;

            if (counter.gameObject.activeSelf)
            {
                counter.rectTransform.DOKill();
                counter.gameObject.SetActive(false);
            }
        }
    }
}
