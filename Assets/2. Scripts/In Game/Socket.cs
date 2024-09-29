public enum Attribute
{
    IsEmpty = 0,
    Power = 1,
    Tech = 2,
    Wealth = 3,
    Charisma = 4,
    Unlinked = 5 //������ �� ����
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
        attribute = Attribute.IsEmpty;
        isActive = false;
    }

    public Socket(Attribute attribute, bool isActive)
    {
        this.attribute = attribute;
        this.isActive = isActive;
    }
}
