using System;
using System.Collections.Generic;
using UnityEngine;

public interface IUIController { }

public class UIManager : MonoBehaviour
{
    #region 싱글톤
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

    Dictionary<Type, IUIController> dict_UIController = new Dictionary<Type, IUIController>();

    public static void RegisterController(Type _Type, IUIController _UIController)
    {
        if (!instance.dict_UIController.ContainsKey(_Type))
        {
            instance.dict_UIController.Add(_Type, _UIController);
        }
        else
        {
            Debug.Log("이미 추가되어있음");
        }
    }

    public static T GetUI<T>() where T : IUIController
    {
        Type type = typeof(T);

        if (instance.dict_UIController[type] is T controller)
        {
            return controller;
        }
        else
        {
            throw new KeyNotFoundException($"{type}에 해당하는 컨트롤러가 없거나 타입이 일치하지 않습니다.");
        }
    }
}
