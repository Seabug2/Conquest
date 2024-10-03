using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LineMessage : MonoBehaviour, IUIController
{
    [SerializeField] GameObject root;             // �޽��� ��� ��Ʈ ������Ʈ
    [SerializeField] RectTransform line;          // ����(�ִϸ��̼��� RectTransform)
    [SerializeField] Text text;                   // �޽����� ����� Text UI

    Sequence sequence;                            // DOTween ������

    [SerializeField] Ease ease = Ease.InQuart;                   // �޽����� ����� Text UI
    [SerializeField, Range(0f, 0.5f)] float inOutTime = 0.1f;                   // �޽����� ����� Text UI
    const float height = 200f;                    // ������ ��ǥ ����
    const int fontSize = 120;                     // �ؽ�Ʈ ��Ʈ ũ��
    string message = string.Empty;                // ����� �޽���
    float duration = 1f;                          // �ִϸ��̼� ���� �ð�

    private void Start()
    {
        Initialize(false);
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }
    }

    // �޽��� �ִϸ��̼��� ������ ó������ �ٽ� �����ϴ� �Լ�
    public void ForcePopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence != null)
        {
            sequence.Kill();
        }

        CreateSequence();
    }

    // �޽����� �˾��ϴ� �Լ�
    public void PopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence == null || !sequence.IsActive() || !sequence.IsPlaying())
        {
            CreateSequence(); // �������� ó������ ���
        }
    }

    // �������� ��� ������ Ȯ���ϴ� �Լ�
    public bool IsPlaying()
    {
        return sequence != null && sequence.IsActive() && sequence.IsPlaying();
    }

    // �ʱ�ȭ �Լ�
    private void Initialize(bool setActive)
    {
        // �ʱ� ���� ����
        line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
        text.fontSize = fontSize;
        text.text = message;
        root.SetActive(setActive);
    }

    // ������ ���� �Լ�
    private void CreateSequence()
    {
        // ������ ���� �� AutoKill ��Ȱ��ȭ
        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                Initialize(true);
            })
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, height), duration * inOutTime).SetEase(ease)) // ���� Ȯ��
            .AppendInterval(duration * 0.8f) // ��� ���
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, 0), duration * inOutTime).SetEase(ease)) // ���� ���
            .OnComplete(() =>
            {
                root.SetActive(false);
            });
    }

    public void Show()
    {
        PopUp("�׽�Ʈ �޽���", 2f); // UI ��ư�� ����� �Լ�
    }
    public void ForceShow()
    {
        ForcePopUp("�׽�Ʈ �޽���", 2f); // UI ��ư�� ����� �Լ�
    }
}