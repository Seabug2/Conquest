using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour, IUIController
{
    [SerializeField, Header("기본 색상")]
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
            UIManager.RegisterController(this.GetType(), this);
        }
    }

    public void In(float duration = 1)
    {
        if (!gameObject.activeSelf) return;
        isPlaying = true;
        image.DOKill();

        // 1 => 0
        image.DOFade(0, duration)
            .OnComplete(() =>
            {
                isPlaying = false;
                gameObject.SetActive(false);
            });
    }

    public void Out(float duration = 1, float r = 0, float g = 0, float b = 0)
    {
        image.DOKill();
        image.color = new Color(r, g, b, 0);
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        isPlaying = true;
        // 0 => 1
        image.DOFade(1, duration)
            .OnComplete(() =>
            {
                isPlaying = false;
                gameObject.SetActive(false);
            });
    }

    bool isPlaying = false;

    public bool IsPlaying() => isPlaying;
}
