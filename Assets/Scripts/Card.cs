using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public enum Type
{
    isNull = 0,
    power,
    tech,
    rich,
    charisma
}

public class Socket
{
    public Socket type;
    public int edgePosition;
    public bool isFilled;
    public Card lickedCard;

    public int LinkableNumber()
    {
        return (edgePosition + 2) % 4;
    }
}

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //소켓에 대한 정보
    public Socket[] sockets = new Socket[4];

    //연결된 카드

    /// <summary>
    /// 카드가 전개 되었을 때 호출
    /// </summary>
    public void OnLinked()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //손에 들고 있을 경우 살짝 위로 올라온다
        //카드의 세부정보가 나온다
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //초기화, 비활성화
        throw new System.NotImplementedException();
    }

    private void OnDisable()
    {
        
    }
}
