using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YourNumber : MonoBehaviour
{
    Text text;

    //게임 매니저의 이벤트에 등록하여 사용
    public void Init()
    {
        if (text == null)
        {
            text = GetComponent<Text>();
            text.text = string.Empty;
        }

        string str = "";
        int order = GameManager.LocalPlayer.Order;
        switch (order)
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
