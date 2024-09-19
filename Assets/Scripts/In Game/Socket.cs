using UnityEngine;

public enum Attribute
{
    isEmpty = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4,
    isLocked = 5 //������ �� ����
}

[System.Serializable]
public class Socket
{
    /// <summary>
    /// ������ �Ӽ�
    /// </summary>
    public Attribute attribute = Attribute.isEmpty;

    /// <summary>
    /// ���� ������ ����Ǿ����� 
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
