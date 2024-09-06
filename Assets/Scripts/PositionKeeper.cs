using UnityEngine;
using DG.Tweening;


/// <summary>
/// Ȱ��ȭ �Ǿ��ִ� ���� ������ ��ġ�� �̵��մϴ�. (Used FixedUpdate)
/// </summary>
public class PositionKeeper : MonoBehaviour
{
    /// <summary>
    /// �������� �п� �߰��� ��
    /// �������� �ʵ忡 ���� ��
    /// ������ �ǵ��ư� ��
    /// ������ ������ ��
    /// </summary>
    public Vector3 targetPosition = new Vector3();
    const float speed = 7f;

    private void Awake()
    {
        //�ʱⰪ
        targetPosition = transform.position;
        enabled = false;
    }

    private Tween tween;
    public void MoveToTargetPosition()
    {
        // ���� Ʈ���� �����Ѵٸ� �̸� �����Ͽ� ���ο� Ʈ���� ���۵� �� �浹�� ����
        if (tween != null && tween.IsActive())
        {
            tween.Kill();
        }

        // ��� ��ġ�� �̵��ϴ� Ʈ�� ���� �� ����
        tween = transform.DOMove(targetPosition, 1f)
            .SetEase(Ease.OutQuart)  // �ִϸ��̼��� ��¡ ����
            .OnComplete(OnMoveComplete);  // �̵� �Ϸ� �� ȣ��� �޼��� ����
    }

    // �̵� �Ϸ� �� ȣ��� �޼���
    private void OnMoveComplete()
    {
        Debug.Log("�̵��� �Ϸ�Ǿ����ϴ�!");
        // �߰��� �Ϸ� ���� ������ ���⿡ �ۼ��� �� �ֽ��ϴ�.
    }
}
