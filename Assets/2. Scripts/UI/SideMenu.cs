using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

[Serializable]
public class ButtonInfo
{
    public Button button;
    public RectTransform rectTransform;
    public Text text;
    public Image image;
    public int num;

    public void Initialize(int index, Action<int> onClickAction)
    {
        num = index;
        text.text = $"{index + 1}p";
        button.interactable = false;
        rectTransform.sizeDelta = new Vector2(100, 100f);
        rectTransform.localScale = Vector3.one;
        image.color = Color.white;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickAction(num));
    }

    public void SetInteractable(bool isActive)
    {
        button.interactable = isActive;
    }

    public void SetSize(Vector2 size, float duration)
    {
        rectTransform.DOKill();
        rectTransform.DOSizeDelta(size, duration).SetEase(Ease.OutQuad);
    }

    public void SetScale(Vector3 scale)
    {
        rectTransform.localScale = scale;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }
}

/*
 * 사이드 버튼의 목적은 카메라 조작
 */
public class SideMenu : MonoBehaviour, IUIController
{
    [SerializeField]
    private ButtonInfo[] buttonInfos;

    [SerializeField]
    private RectTransform yourMark;

    [SerializeField]
    private Vector3 selectedScale = new Vector3(1.3f, 1.3f, 1.3f);

    [SerializeField]
    private Color selectedColor = Color.yellow;

    private int localOrder = 0;
    private CameraController cameraController;
    private GameManager gameManager;

    private void Awake()
    {
        cameraController = CameraController.instance;
        gameManager = GameManager.instance;
    }

    private void Start()
    {
        if (GameManager.instance != null)
        {
            gameManager = GameManager.instance;
        }

        //if (UIManager.instance != null)
        //{
        //    UIManager.RegisterController(GetType(), this);
        //}

        cameraController = CameraController.instance;
    }

    public void Initialize()
    {
        gameManager = GameManager.instance;

        if (yourMark != null)
        {
            yourMark.gameObject.SetActive(false);
        }

        for (int i = 0; i < buttonInfos.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            buttonInfos[i].Initialize(index, OnButtonClicked);
        }

        if (cameraController != null)
        {
            cameraController.MoveEvent += OnCameraMoved;
            cameraController.LockEvent += ToggleButtons;
        }

        if (gameManager != null)
        {
            gameManager.TurnChangeEvent += OnTurnChanged;
        }

        SetLocalButton();
    }

    private void SetLocalButton()
    {
        if (gameManager == null || yourMark == null) return;

        localOrder = GameManager.LocalPlayer.Order;
        ButtonInfo localButton = buttonInfos[localOrder];

        localButton.button.onClick.RemoveAllListeners();
        localButton.button.onClick.AddListener(OnLocalButtonClicked);

        Vector3 offset = yourMark.anchoredPosition;
        yourMark.SetParent(localButton.rectTransform);
        yourMark.anchoredPosition = offset;
        yourMark.gameObject.SetActive(true);
    }

    private void OnButtonClicked(int buttonIndex)
    {
        if (cameraController == null || gameManager == null) return;

        if (cameraController.CurrentCamIndex == buttonIndex)
        {
            if (gameManager.CurrentPhase == GamePhase.DraftPhase)
            {
                cameraController.FocusOnCenter();
            }
        }
        else
        {
            cameraController.FocusOnPlayerField(buttonIndex);
        }
    }

    private void OnLocalButtonClicked()
    {
        if (cameraController == null || gameManager == null) return;

        if (gameManager.CurrentPhase == GamePhase.DraftPhase)
        {
            if (cameraController.CurrentCamIndex == localOrder)
            {
                cameraController.FocusOnCenter();
            }
            else
            {
                cameraController.FocusOnHome();
            }
        }
        else if (gameManager.CurrentPhase == GamePhase.PlayerPhase)
        {
            if (GameManager.LocalPlayer.isMyTurn && cameraController.CurrentCamIndex == localOrder)
            {
                GameManager.LocalPlayer.ClientEndTurn();
            }
            else if (cameraController.CurrentCamIndex != localOrder)
            {
                cameraController.FocusOnHome();
            }
        }
    }

    private void ToggleButtons(bool isActive)
    {
        foreach (var buttonInfo in buttonInfos)
        {
            buttonInfo.SetInteractable(isActive);
            Vector2 targetSize = isActive ? new Vector2(220, 100) : new Vector2(100, 100);
            buttonInfo.SetSize(targetSize, 0.8f);
        }
    }

    private void OnCameraMoved(int selectedNum)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            Vector3 scale = (i == selectedNum) ? selectedScale : Vector3.one;
            buttonInfos[i].SetScale(scale);
        }

        if (selectedNum == localOrder)
        {
            if (gameManager.CurrentPhase == GamePhase.PlayerPhase && GameManager.LocalPlayer.isMyTurn)
            {
                buttonInfos[localOrder].SetText("END");
            }
        }
        else
        {
            buttonInfos[localOrder].SetText($"{localOrder + 1}p");
        }
    }

    private void OnTurnChanged(int currentPlayerOrder)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            Color color = (i == currentPlayerOrder) ? selectedColor : Color.white;
            buttonInfos[i].SetColor(color);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (cameraController != null)
        {
            cameraController.MoveEvent -= OnCameraMoved;
            cameraController.LockEvent -= ToggleButtons;
        }

        if (gameManager != null)
        {
            gameManager.TurnChangeEvent -= OnTurnChanged;
        }
    }
}
