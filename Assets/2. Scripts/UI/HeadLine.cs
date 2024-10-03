using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HeadLine : MonoBehaviour, IUIController
{
    [SerializeField] Text text;
    [SerializeField] Transform root;

    private void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(this.GetType(), this);
        }
        text.text = string.Empty;
        root.localScale = new Vector3(0, 1, 1);
    }

    public void On(float duration = 1f)
    {
        root.DOScaleX(1, duration).SetEase(Ease.OutQuart);
    }

    [Header("출력 시간"), Space(10f)]
    public float duration = 1.8f;

    public void Print(string _text)
    {
        if (isStatic) return;

        text.DOKill();
        text.text = string.Empty;
        text.DOText(_text, duration);
    }

    bool isStatic = false;

    public void ForcePrint(string _text)
    {
        isStatic = true;

        text.DOKill();
        text.text = string.Empty;
        text.DOText(_text, duration);
    }

    public void Off(float duration = 1f)
    {
        isStatic = false;
        text.DOKill();
        text.text = string.Empty;
        root.DOScaleX(0, duration)
            .SetEase(Ease.InQuart)
            .OnComplete(() => root.gameObject.SetActive(false));
    }
    public int localPlayerOrder;
    public void IsMyTurn(int currentPlayerOrder)
    {
        bool isMyTurn = localPlayerOrder == currentPlayerOrder;
        string message = isMyTurn ? "당신의 차례": $"플레이어 {currentPlayerOrder + 1}의 차례";
        Print(message);
    }
}
