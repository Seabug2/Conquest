using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UINaviContorller : MonoBehaviour
{
    public InputField[] inputFields;
    public Button button;

    int i = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                i = Mathf.Clamp(i - 1, 0, inputFields.Length - 1);
                EventSystem.current.SetSelectedGameObject(inputFields[i].gameObject);
                return;
            }

            i = Mathf.Clamp(i + 1, 0, inputFields.Length - 1);
            EventSystem.current.SetSelectedGameObject(inputFields[i].gameObject);
            return;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

            i = Mathf.Clamp(i - 1, 0, inputFields.Length - 1);
            EventSystem.current.SetSelectedGameObject(inputFields[i].gameObject);
            return;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            i = Mathf.Clamp(i + 1, 0, inputFields.Length - 1);
            EventSystem.current.SetSelectedGameObject(inputFields[i].gameObject);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            button.onClick.Invoke();
            return;
        }
    }
}
