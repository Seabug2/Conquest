using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CompleteLink : MonoBehaviour, IUIController
{
    [SerializeField]
    GameObject root;
    [SerializeField]
    Image[] imgs;

    Vector3[] positions;

    private void Awake()
    {
        int length = imgs.Length;
        positions = new Vector3[length];

        for (int i = 0; i < length; i++)
        {
            imgs[i].gameObject.SetActive(false);
            positions[i] = imgs[i].rectTransform.anchoredPosition;
        }
    }

    void Start()
    {
        if (UIManager.instance != null)
        {
            UIManager.RegisterController(GetType(), this);
        }

        IsPlaying = false;
    }

    public bool IsPlaying { get; private set; }

    Sequence sequence;

    public void PopUp(int[] ids)
    {
        root.SetActive(true);

        if (GameManager.instance != null)
        {
            if (UIManager.GetUI<Timer>().IsPlaying())
            {
                UIManager.GetUI<Timer>().Stop();
            }
        }

        int length = ids.Length;

        int center = Random.Range(0, length);

        for (int i = 0; i < length; i++)
        {
            imgs[i].sprite = GameManager.Card(ids[i]).front;
            imgs[i].rectTransform.anchoredPosition = positions[center];
            imgs[i].gameObject.SetActive(true);
            imgs[i].rectTransform.DOAnchorPos(positions[i], .123f * (i + 1)).SetEase(Ease.OutQuad);
        }
    }
}
