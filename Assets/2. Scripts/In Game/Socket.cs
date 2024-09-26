public enum Attribute
{
    isEmpty = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4,
    unlinked = 5 //������ �� ����
}

[System.Serializable]
public class Socket
{
    /// <summary>
    /// ������ �Ӽ�
    /// </summary>
    public Attribute attribute;

    /// <summary>
    /// ���� ������ ����Ǿ����� 
    /// </summary>
    public bool isActive;

    public Socket()
    {
        attribute = Attribute.isEmpty;
        isActive = false;
    }

    public Socket(Attribute attribute, bool isActive)
    {
        this.attribute = attribute;
        this.isActive = isActive;
    }
}
