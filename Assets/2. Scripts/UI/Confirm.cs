using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class Confirm : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    RectTransform block;
    [SerializeField]
    RectTransform root;
    [SerializeField]
    Text message;
    [SerializeField]
    Image cardImage;

    [SerializeField]
    RectTransform startPosition;
    [SerializeField]
    RectTransform targetPosition;
    [SerializeField]
    RectTransform exitPosition;

    [SerializeField]
    ButtonAction bt_yes;
    [SerializeField]
    ButtonAction bt_no;

    public bool IsActive => block.gameObject.activeSelf;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        block.gameObject.SetActive(false);
    }

    public void PopUp(Action _onClickEvent, string _message, Sprite _cardImage = null)
    {
        message.text = _message;
        cardImage.sprite = _cardImage;
        canvasGroup.alpha = 0;
        root.anchoredPosition = startPosition.anchoredPosition;
        bt_yes.enabled = false;
        bt_yes.OnClickEvent = () =>
        {
            _onClickEvent();
            Close();
        };

        bt_no.enabled = false;
        bt_no.OnClickEvent = Close;

        block.gameObject.SetActive(true);
        
        root.DOKill();
        root.DOAnchorPos(targetPosition.anchoredPosition, 0.5f).SetEase(Ease.OutQuart);
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            bt_yes.enabled = true;
            bt_no.enabled = true;
        });
    }


    public void Close()
    {
        bt_yes.enabled = false;
        bt_no.enabled = false;
        root.DOAnchorPos(exitPosition.anchoredPosition, 0.3f).SetEase(Ease.OutQuart);
        canvasGroup.DOFade(0, 0.3f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            block.gameObject.SetActive(false);
        });
    }

    public void Test()
    {
        PopUp(() => print("테스트!"), "테스트sdfghmn!", null);
    }
}
