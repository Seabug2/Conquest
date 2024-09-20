using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TurnGuide : MonoBehaviour
{
    [SerializeField]
    Text phase;
    [SerializeField]
    Text order;
    [SerializeField]
    RectTransform root;

    [SerializeField, Space(10)]
    RectTransform startPosition;
    [SerializeField]
    RectTransform targetPosition;

    private void Start()
    {
        //root.anchoredPosition = startPosition.anchoredPosition;
    }

    public void Open()
    {
        phase.text = "���� ������ �ð�!";
        order.text = "1p�� ����";
        root.DOAnchorPos(targetPosition.anchoredPosition, .8f).SetEase(Ease.OutQuad);
    }

    public void TurnChange(string _turnName, int _order)
    {
        Sequence sequence = DOTween.Sequence()
            .Append(root.DOAnchorPos(startPosition.anchoredPosition, .8f).SetEase(Ease.OutQuad))
            .AppendCallback(() =>
        {
            phase.text = _turnName;
            order.text = $"{_order}�� ����";
        })
        .Append(root.DOAnchorPos(targetPosition.anchoredPosition, .8f).SetEase(Ease.OutQuad));
    }
}
