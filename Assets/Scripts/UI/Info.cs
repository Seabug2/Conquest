using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Canvas))]
public class Info : MonoBehaviour
{
    readonly Queue<Image> infos = new Queue<Image>();

    public Image prefab;

    [SerializeField, Header("시작 위치")]
    RectTransform startPosition;
    [SerializeField, Header("목표 위치")]
    RectTransform targetPosition;
    [SerializeField, Header("퇴장 위치")]
    RectTransform exitPosition;

    Color startColor = new Color(1, 1, 1, 0);

    public float popupDuration = 0.5f;
    public float exitDuration = 0.5f;
    const Ease ease = Ease.OutCirc;

    Image currentInfo = null;

    public void PopUp(Sprite sprt)
    {
        if (currentInfo != null)
        {
            if (currentInfo.sprite.Equals(sprt)) return;
            Enqueue(currentInfo);
        }


        if (infos.Count.Equals(0))
        {
            currentInfo = Instantiate(prefab, this.transform);
        }
        else
        {
            currentInfo = infos.Dequeue();
        }

        currentInfo.sprite = sprt;
        currentInfo.color = startColor;
        currentInfo.rectTransform.anchoredPosition = startPosition.anchoredPosition;
        currentInfo.gameObject.SetActive(true);

        currentInfo.rectTransform.DOAnchorPos(targetPosition.anchoredPosition, popupDuration).SetEase(ease);
        currentInfo.DOFade(1, popupDuration).SetEase(ease);
    }

    public void PopUp()
    {
        if (currentInfo != null)
        {
            Enqueue(currentInfo);
        }


        if (infos.Count.Equals(0))
        {
            currentInfo = Instantiate(prefab, this.transform);
        }
        else
        {
            currentInfo = infos.Dequeue();
        }

        currentInfo.color = startColor;
        currentInfo.rectTransform.anchoredPosition = startPosition.anchoredPosition;
        currentInfo.gameObject.SetActive(true);

        currentInfo.rectTransform.DOAnchorPos(targetPosition.anchoredPosition, popupDuration).SetEase(ease);
        currentInfo.DOFade(1, popupDuration).SetEase(ease);
    }

    public void Enqueue(Image currentInfo)
    {
        currentInfo.rectTransform.DOKill();
        currentInfo.DOKill();
        currentInfo.rectTransform.DOAnchorPos(exitPosition.anchoredPosition, exitDuration).SetEase(ease);
        currentInfo.DOFade(0, exitDuration).SetEase(ease).
            OnComplete(() =>
            {
                infos.Enqueue(currentInfo);
                currentInfo.gameObject.SetActive(false);
            });
    }
}