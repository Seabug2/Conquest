using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    [SerializeField]
    Fade fade;
    public Fade Fade => fade;

    [SerializeField]
    LineMessage lineMessage;
    public LineMessage LineMessage => lineMessage;
}
