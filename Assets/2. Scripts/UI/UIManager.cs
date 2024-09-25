using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region ½Ì±ÛÅæ
    public static UIManager instance;

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
    #endregion

    [SerializeField]
    Fade fade;
    public static Fade Fade => instance.fade;

    [SerializeField]
    LineMessage message;
    public  static LineMessage Message => instance.message;

    [SerializeField]
    Info infoUI;
    public static Info InfoUI => instance.infoUI;

    [SerializeField]
    Confirm confirm;
    public static Confirm Confirm => instance.confirm ;

    [SerializeField]
    HeadLine headLine;
    public static HeadLine HeadLine => instance.headLine;
}
