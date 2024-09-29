public enum Attribute
{
    IsEmpty = 0,
    Power = 1,
    Tech = 2,
    Wealth = 3,
    Charisma = 4,
    Unlinked = 5 //연결할 수 없음
}

[System.Serializable]
public class Socket
{
    /// <summary>
    /// 소켓의 속성
    /// </summary>
    public Attribute attribute;

    /// <summary>
    /// 현재 소켓이 연결되었는지 
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
