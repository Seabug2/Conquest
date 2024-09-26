public enum Attribute
{
    isEmpty = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4,
    unlinked = 5 //연결할 수 없음
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
        attribute = Attribute.isEmpty;
        isActive = false;
    }

    public Socket(Attribute attribute, bool isActive)
    {
        this.attribute = attribute;
        this.isActive = isActive;
    }
}
