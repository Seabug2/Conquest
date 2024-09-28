using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LineMessage : MonoBehaviour, IUIController
{
    [SerializeField] GameObject root;             // 메시지 출력 루트 오브젝트
    [SerializeField] RectTransform line;          // 라인(애니메이션할 RectTransform)
    [SerializeField] Text text;                   // 메시지를 출력할 Text UI

    Sequence sequence;                            // DOTween 시퀀스

    [SerializeField] Ease ease = Ease.InQuart;                   // 메시지를 출력할 Text UI
    [SerializeField, Range(0f, 0.5f)] float inOutTime = 0.1f;                   // 메시지를 출력할 Text UI
    const float height = 200f;                    // 라인의 목표 높이
    const int fontSize = 120;                     // 텍스트 폰트 크기
    string message = string.Empty;                // 출력할 메시지
    float duration = 1f;                          // 애니메이션 지속 시간

    private void Start()
    {
        Initialize(false);
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(this.GetType(), this);
        }
    }

    // 메시지 애니메이션을 강제로 처음부터 다시 실행하는 함수
    public void ForcePopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence != null && sequence.IsPlaying())
        {
            sequence.Restart(); // 트윈을 처음부터 다시 시작
        }
        else
        {
            CreateSequence(); // 새 시퀀스 생성
            sequence.Restart(); // 처음부터 실행
        }
    }

    // 메시지를 팝업하는 함수
    public void PopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        if (sequence == null || !sequence.IsPlaying())
        {
            CreateSequence(); // 시퀀스를 처음부터 재생
        }
    }

    // 메시지를 팝업하는 함수
    public void FreezePopUp(string message, float duration)
    {
        this.message = message;
        this.duration = duration;

        CreateFreezeSequence(); // 시퀀스를 처음부터 재생
    }

    // 시퀀스가 재생 중인지 확인하는 함수
    public bool IsPlaying()
    {
        return sequence != null && sequence.IsActive() && sequence.IsPlaying();
    }

    // 초기화 함수
    private void Initialize(bool _SerActive)
    {
        // 초기 상태 설정
        line.sizeDelta = new Vector2(line.sizeDelta.x, 0);
        text.fontSize = fontSize;
        text.text = message;
        root.SetActive(_SerActive);
    }

    // 시퀀스 생성 함수
    private void CreateSequence()
    {
        // 시퀀스 생성 및 AutoKill 비활성화
        sequence = DOTween.Sequence()
            .SetAutoKill(false) // 트윈이 완료된 후에도 파괴되지 않음
            .OnStart(() =>
            {
                Initialize(true);
            })
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, height), duration * inOutTime).SetEase(ease)) // 라인 확장
            .AppendInterval(duration * 0.8f) // 잠시 대기
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, 0), duration * inOutTime).SetEase(ease)) // 라인 축소
            .OnComplete(() =>
            {
                root.SetActive(false);
            });
    }

    private void CreateFreezeSequence()
    {
        // 시퀀스 생성 및 AutoKill 비활성화
        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                Initialize(true);
            })
            .Append(line.DOSizeDelta(new Vector2(line.sizeDelta.x, height), duration).SetEase(ease));
    }

    public void Show()
    {
        PopUp("테스트 메시지", 2f); // UI 버튼과 연결된 함수
    }
    public void ForceShow()
    {
        ForcePopUp("테스트 메시지", 2f); // UI 버튼과 연결된 함수
    }
}
