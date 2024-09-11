using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    RectTransform rect;
    [SerializeField]
    Image img;

    Sequence inactive;
    Sequence active;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowCardInfo(Sprite sprt)
    {
        img.sprite = sprt;
        gameObject.SetActive(true);
    }

    public void Disable()
    {

    }
}
