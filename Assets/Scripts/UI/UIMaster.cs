using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMaster : MonoBehaviour
{
    public static UIMaster instance;

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
    public static Fade Fade => instance.fade;

    [SerializeField]
    LineMessage lineMessage;
    public  static LineMessage LineMessage => instance.lineMessage;

    [SerializeField]
    InfoUIController infoUI;
    public static InfoUIController InfoUI => instance.infoUI;
}
