using UnityEngine;

public enum Attribute
{
    isNull = 0,
    power,
    tech,
    wealth,
    charisma
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

    //전개된 빌런피스의 특정 소켓에 연결된 피스는
    //피스가 놓여진 타일을 기준으로 찾도록 한다.
}
