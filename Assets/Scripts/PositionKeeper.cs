using UnityEngine;
using DG.Tweening;


/// <summary>
/// 활성화 되어있는 동안 설정한 위치로 이동합니다. (Used FixedUpdate)
/// </summary>
public class PositionKeeper : MonoBehaviour
{
    /// <summary>
    /// 누군가의 패에 추가될 때
    /// 누군가의 필드에 놓을 때
    /// 덱으로 되돌아갈 때
    /// 덱에서 꺼내질 때
    /// </summary>
    public Vector3 targetPosition = new Vector3();
    const float speed = 7f;

    private void Awake()
    {
        //초기값
        targetPosition = transform.position;
        enabled = false;
    }

    private Tween tween;
    public void MoveToTargetPosition()
    {
        // 현재 트윈이 존재한다면 이를 중지하여 새로운 트윈이 시작될 때 충돌을 방지
        if (tween != null && tween.IsActive())
        {
            tween.Kill();
        }

        // 대상 위치로 이동하는 트윈 생성 및 저장
        tween = transform.DOMove(targetPosition, 1f)
            .SetEase(Ease.OutQuart)  // 애니메이션의 이징 설정
            .OnComplete(OnMoveComplete);  // 이동 완료 시 호출될 메서드 설정
    }

    // 이동 완료 시 호출될 메서드
    private void OnMoveComplete()
    {
        Debug.Log("이동이 완료되었습니다!");
        // 추가로 완료 후의 동작을 여기에 작성할 수 있습니다.
    }
}
