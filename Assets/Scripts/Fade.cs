using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour
{
    [SerializeField, Header("�⺻ ����")]
    Color defualtColor = Color.black;

    [SerializeField, Header("�̹����� Ȱ��ȭ�� ���·� �����մϴ�.")]
    bool isActiveOnStart = true;

    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = defualtColor;
        image.enabled = true;

        gameObject.SetActive(isActiveOnStart);
    }

    public void In(float duration = 1)
    {
        if (!gameObject.activeSelf) return;

        image.DOKill();

        // 1 => 0
        image.DOFade(0, duration)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void Out(float duration = 1, float r = 0, float g = 0, float b = 0)
    {
        image.DOKill();
        image.color = new Color(r, g, b, 0);
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // 0 => 1
        image.DOFade(1, duration);
    }
}
