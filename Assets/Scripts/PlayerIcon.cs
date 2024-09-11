using UnityEngine;

public class PlayerIcon : MonoBehaviour
{
    [SerializeField]
    RectTransform[] buttons;
    public void ScaleUp(int selectButtonNumber)
    {
        CameraController.instance.SetVCam(selectButtonNumber);

        for(int i = 0; i < buttons.Length; i++)
        {
            if (i == selectButtonNumber)
                buttons[i].localScale = Vector3.one * 1.2f;
            else 
                buttons[i].localScale = Vector3.one * 0.9f;
        }
    }
}
