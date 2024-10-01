using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public class ButtonInfo
{
    public Button button;
    public RectTransform rectTransform;
    public Text text;
    public Image image; // �ʿ��ϴٸ� �߰�
    public int num;

    public void OnClickEvent()
    {
        if (CameraController.instance.CurrentCamIndex == num)
        {
            if (GameManager.instance.CurrentPhase.Equals(GamePhase.DraftPhase))
            {
                CameraController.instance.FocusOnCenter();
            }
        }
        else
        {
            CameraController.instance.FocusOnPlayerField(num);
        }
    }
}

/*
 * ���̵� ��ư�� ������ ī�޶� ����
 */
public class SideMenu : MonoBehaviour, IUIController
{
    [SerializeField]
    private ButtonInfo[] buttonInfos;

    [SerializeField] RectTransform yourMark;

    int localOrder = 0;

    private void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }

        Initialize();
    }

    public void Initialize()
    {
        yourMark.gameObject.SetActive(false);

        for (int i = 0; i < buttonInfos.Length; i++)
        {
            buttonInfos[i].button.interactable = false;
            buttonInfos[i].rectTransform.sizeDelta = new Vector2(100, 100f);
            buttonInfos[i].rectTransform.localScale = Vector3.one;
            buttonInfos[i].image.color = Color.white;
            buttonInfos[i].button.onClick.AddListener(buttonInfos[i].OnClickEvent);
            buttonInfos[i].num = i;
            buttonInfos[i].text.text = $"{i + 1}p";
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.MoveEvent += Selected;
            CameraController.instance.LockEvent += Toggle;
        }
        if(GameManager.instance != null)
        {
            GameManager.instance.TurnChangeEvent += CurrentTurn;
        }
    }

    /// <see cref="GameManager.OnStartEvent"/>�� ����Ͽ� ���
    public void SetLocalButton()
    {
        localOrder = GameManager.LocalPlayer.Order;

        ButtonInfo localButton = buttonInfos[localOrder];
        localButton.button.onClick.RemoveAllListeners();
        localButton.button.onClick.AddListener(OnClickMyButton);

        Vector3 localPosition = yourMark.localPosition;
        yourMark.transform.SetParent(localButton.rectTransform.transform);
        yourMark.localPosition = localPosition;
        yourMark.gameObject.SetActive(true);
    }



    private void OnClickMyButton()
    {
        CameraController cameraController = CameraController.instance;
        GameManager gameManager = GameManager.instance;

        if (gameManager.CurrentPhase.Equals(GamePhase.DraftPhase))
        {
            if (cameraController.CurrentCamIndex == localOrder)
                cameraController.FocusOnCenter();
            else
                cameraController.FocusOnHome();
        }
        else if (gameManager.CurrentPhase.Equals(GamePhase.PlayerPhase))
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

    //ī�޶� ���� ��� ��ư�� �۾���
    void Toggle(bool isActive)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            buttonInfos[i].button.interactable = isActive;
            
            buttonInfos[i].rectTransform.DOKill();
            Vector2 targetSize = isActive ? new Vector2(220, 100) : new Vector2(100, 100);
            buttonInfos[i].rectTransform.DOSizeDelta(targetSize, 0.8f).SetEase(Ease.OutQuad);
        }
    }

    //ī�޶� �̵��ϸ� ���� ���� �ִ� ȭ���� ��ư�� ��������� ����
    void Selected(int selectedNum)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            buttonInfos[i].rectTransform.localScale = i == selectedNum ? selectedScale : Vector3.one;
            //buttonInfos[i].image.color = i == selectedNum ? selectedColor : Color.white;
        }

        if(selectedNum == localOrder)
        {
            if(GameManager.instance.CurrentPhase.Equals(GamePhase.PlayerPhase) && GameManager.LocalPlayer.isMyTurn)
            buttonInfos[localOrder].text.text = "END";
        }
        else
        {
            buttonInfos[localOrder].text.text = $"{localOrder + 1}p";
        }
    }

    void CurrentTurn(int num)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            buttonInfos[i].image.color = i == num ? selectedColor : Color.white;
        }
    }

    //���� ���� �ִ� ȭ���� ���...
    public Vector3 selectedScale = new(1.3f, 1.3f, 1.3f);
    //���� ������ ���
    public Color selectedColor = Color.yellow;
}
