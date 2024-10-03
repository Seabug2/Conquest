using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YourNumber : MonoBehaviour, IUIController
{
    Text text;

    private void Start()
    {
        UIManager.RegisterController(GetType(), this);
    }

    //���� �Ŵ����� �̺�Ʈ�� ����Ͽ� ���
    public void Init(int localOrder)
    {
        if (text == null)
        {
            text = GetComponent<Text>();
            text.text = string.Empty;
        }

        string str = "";
        switch (localOrder)
        {
            case 0:
                str = "1st";
                break;
            case 1:
                str = "2nd";
                break;
            case 2:
                str = "3rd";
                break;
            case 3:
                str = "4th";
                break;
        }

        text.text = $"You are\n{str} Player";
    }

    public void GameOver()
    {
        if (text == null)
        {
            text = GetComponent<Text>();
            text.text = string.Empty;
        }

        text.text = "Game Over";
    }
}
