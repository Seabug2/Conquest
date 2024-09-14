using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LineMessage : MonoBehaviour
{
    [SerializeField] GameObject root;             // �޽��� ��� ��Ʈ ������Ʈ
    [SerializeField] RectTransform line;          // ����(�ִϸ��̼��� RectTransform)
    [SerializeField] Text text;                   // �޽����� ����� Text UI

    Sequence sequence;                            // DOTween ������

    const float height = 200f;                    // ������ ��ǥ ����
    const int fontSize = 120;                     // �ؽ�Ʈ ��Ʈ ũ��
    string message = string.Empty;                // ����� �޽���
    float duration = 1f;                          // �ִϸ��̼� ���� �ð�

    private void Start()
    {
        root.SetActive(false);
    }

    // �޽��� �ִϸ��̼��� ������ ó������ �ٽ� �����ϴ� �Լ�
    public void ForcePopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence != null && sequence.IsPlaying())
        {
            sequence.Restart(); // Ʈ���� ó������ �ٽ� ����
        }
        else
        {
            CreateSequence(); // �� ������ ����
            sequence.Restart(); // ó������ ����
        }
    }

    // �޽����� �˾��ϴ� �Լ�
    public void PopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence == null || !sequence.IsPlaying())
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
    private void Initialize()
    {
        // �ʱ� ���� ����
        line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
        text.fontSize = fontSize;
        text.text = message;
        root.SetActive(true);
    }

    // ������ ���� �Լ�
    private void CreateSequence()
    {
        // ������ ���� �� AutoKill ��Ȱ��ȭ
        sequence = DOTween.Sequence()
            .SetAutoKill(false) // Ʈ���� �Ϸ�� �Ŀ��� �ı����� ����
            .OnStart(() =>
            {
                Initialize();
            })
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, height), duration * 0.1f).SetEase(Ease.OutCirc)) // ���� Ȯ��
            .AppendInterval(duration * 0.8f) // ��� ���
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, 0), duration * 0.1f).SetEase(Ease.OutCirc)) // ���� ���
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
