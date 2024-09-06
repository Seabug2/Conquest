using UnityEngine;

public enum Attribute
{
    isNull = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4
}

public class Socket : MonoBehaviour
{
    //소켓은 각각 고유의 프리팹 오브젝트

    //소켓의 속성
    public Attribute type;

    //소켓이 비워진 상태라면 이 소켓을 연결하지 못하는 전개는 할 수 없다.
    [SerializeField]
    bool isFilled;
    
    public bool IsFilled
    {
        get { return isFilled; }
        set
        {
            isFilled = value;
            if (isFilled)
            {
                //소켓이 채워진 상태를 표현하는 스프라이트로 교체
            }
            else
            {
                //소켓이 비워진 상태를 표현하는 스프라이트로 교체
            }
        }
    }
}
