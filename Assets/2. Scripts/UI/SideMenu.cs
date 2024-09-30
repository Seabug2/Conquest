using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SideMenu : MonoBehaviour, IUIController
{
    [SerializeField]
    Button[] buttons;

    public Button[] Buttons => buttons;

    RectTransform[] rects;

    private void Start()
    {
        int length = buttons.Length;
        rects = new RectTransform[length];

        for (int i = 0; i < length; i++)
        {
            int num = i;

            buttons[i].interactable = false;
            buttons[i].onClick.AddListener(() => CameraController.instance.Toggle(num));

            rects[i] = buttons[i].GetComponent<RectTransform>();
            rects[i].sizeDelta = new Vector2(100f, 100f);
        }

        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }
        if (CameraController.instance != null)
        {
            CameraController.instance.LockEvent += Toggle;
        }
    }

    public void SetLocalButton(int selectButtonNumber)
    {
        buttons[selectButtonNumber].transform.localScale = Vector3.one * 1.2f;
        buttons[selectButtonNumber].transform.GetComponentInChildren<Text>().text = $"{selectButtonNumber + 1}p : You";
    }

    public void Toggle(bool isActive)
    {
        int length = buttons.Length;
        for (int i = 0; i < length; i++)
        {
            buttons[i].interactable = isActive;

            if (isActive)
            {
                rects[i].DOKill();
                rects[i].DOSizeDelta(new Vector2(200, 100), .8f).SetEase(Ease.OutQuad);
            }
            else
            {
                rects[i].DOKill();
                rects[i].DOSizeDelta(new Vector2(100, 100), .8f).SetEase(Ease.OutQuad);
            }
        }
    }
}
