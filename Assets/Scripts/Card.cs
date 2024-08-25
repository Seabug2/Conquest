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
    //���Ͽ� ���� ����
    public Socket[] sockets = new Socket[4];

    //����� ī��

    /// <summary>
    /// ī�尡 ���� �Ǿ��� �� ȣ��
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
        //�տ� ��� ���� ��� ��¦ ���� �ö�´�
        //ī���� ���������� ���´�
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //�ʱ�ȭ, ��Ȱ��ȭ
        throw new System.NotImplementedException();
    }

    private void OnDisable()
    {
        
    }
}
