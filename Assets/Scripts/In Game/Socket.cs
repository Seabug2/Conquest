using UnityEngine;

public enum Attribute
{
    isEmpty = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4,
    isLocked = 5 //연결할 수 없음
}

[System.Serializable]
public class Socket
{
    /// <summary>
    /// 소켓의 속성
    /// </summary>
    public Attribute attribute = Attribute.isEmpty;

    /// <summary>
    /// 현재 소켓이 연결되었는지 
    /// </summary>
    public bool isLinked = false;

    public Socket()
    {
        attribute = Attribute.isEmpty;
        isLinked = false;
    }

    public Socket(Attribute attribute = Attribute.isEmpty, bool isFilled = false)
    {
        this.attribute = attribute;
        this.isLinked = isFilled;
    }
}
