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
    //������ ���� ������ ������ ������Ʈ

    //������ �Ӽ�
    public Attribute type;

    //������ ����� ���¶�� �� ������ �������� ���ϴ� ������ �� �� ����.
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
                //������ ä���� ���¸� ǥ���ϴ� ��������Ʈ�� ��ü
            }
            else
            {
                //������ ����� ���¸� ǥ���ϴ� ��������Ʈ�� ��ü
            }
        }
    }

    //������ �����ǽ��� Ư�� ���Ͽ� ����� �ǽ���
    //�ǽ��� ������ Ÿ���� �������� ã���� �Ѵ�.
}
