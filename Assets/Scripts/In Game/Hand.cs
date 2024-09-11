using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    public int seatNum;

    [SerializeField] List<Card> list = new List<Card>();

    /// <summary>
    /// �տ� �ִ� ��� ī���� ID�� ��ȯ�մϴ�.
    /// </summary>
    public int[] AllIDs
    {
        get
        {
            int[] ids = new int[list.Count];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = list[i].ID;
            }

            return ids;
        }
    }

    //�⺻ �� �� ����
    const int handsLimit = 6;

    public int LimitStack { get; private set; }

    public void SetLimitStack(int i)
    {
        LimitStack += i;
        if (LimitStack < 0) LimitStack = 0;
    }

    public int HandsLimit => handsLimit + LimitStack;

    public bool IsLimitOver => list.Count > HandsLimit;

    public Card this[int index]
    {
        get
        {
            if (index < 0 || index >= list.Count)
            {
                Debug.Log($"Index ���� ����");
                return null;
            }
            return list[index];
        }
    }

    private void Start()
    {
        LimitStack = 0;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(Card c in list)
            {
                c.handler.OnMouseAction = HandAlignment;
            }

            HandAlignment();
        }
    }

    /// <summary>
    /// ClientRpc�� ȣ�� �� ��
    /// isLoclaPlayer == true �� ���,
    /// �ݵ�� �ش� id�� ī��� handler�� Ȱ��ȭ ������� �Ѵ�.
    /// </summary>
    #region �߰�, ����
    public void Add(int id)
    {
        Add(GameManager.Card(id));
    }

    public void Add(Card drawnCard)
    {
        list.Add(drawnCard);
        drawnCard.handler.OnMouseAction = HandAlignment;
        HandAlignment();
        //ī�带...
    }

    /// <summary>
    /// ClientRpc�� ȣ�� �� ��
    /// �ݵ�� �ش� id�� ī��� handler�� ��Ȱ��ȭ ������� �Ѵ�.
    /// </summary>
    public void Remove(int id)
    {
        Remove(GameManager.Card(id));
    }

    public void Remove(Card drawnCard)
    {
        list.Remove(drawnCard);

        if (list.Count > 0)
            HandAlignment();
    }
    #endregion

    public float radius = 1;
    public float height = 1;
    //ī�� ����
    public float intervalAngle = 18f;
    //�ִ� ī�� ��
    public float maxAngle = 54;

    void HandAlignment()
    {
        float count = list.Count;
        bool isOver = false; //�ִ� ������ ī�带 ���ȴ��� Ȯ���ϴ� bool ��
        float rightEndAngle = intervalAngle * count * .5f; //�� ���� �ִ밢�� ���Ѵ�
        if (rightEndAngle > maxAngle)
        {
            rightEndAngle = maxAngle;
            isOver = true;
        }
        float leftEndAngle = -rightEndAngle; //�ݴ� ���� �ִ밢�� �Ҵ�

        float interval = isOver ? 1f / (count - 1) : 1f / (count + 1); //lerp�� ������ ����

        //���� ���콺�� �÷��� ī�尡 �ִ��� Ȯ��
        int selectedNum = -1;
        for (int i = 0; i < count; i++)
        {
            if (list[i].handler.IsOnMouse)
            {
                selectedNum = i;
                break;
            }
        }

        for (int i = 0; i < count; i++)
        {
            list[i].sprtRend.sortingOrder = 1 + i; //ī���� Sorting Order�� i������ �Ҵ�
            float angle;

            if (selectedNum < 0 || selectedNum >= count) //���콺�� �÷��� ī�尡 ���� ��
            {
                angle = Mathf.Lerp(leftEndAngle, rightEndAngle, interval * (i + (isOver ? 0 : 1)));
            }
            else
            {
                float selectedAngle = Mathf.Lerp(leftEndAngle, rightEndAngle, interval * (selectedNum + (isOver ? 0 : 1)));

                if (i < selectedNum)
                {
                    angle = Mathf.Lerp(leftEndAngle - count, selectedAngle - count, (float)i / selectedNum);
                }
                else if (i > selectedNum)
                {
                    angle = Mathf.Lerp(selectedAngle + count, rightEndAngle + count, (float)(i - selectedNum) / (count - 1 - selectedNum));
                }
                else
                {
                    continue;
                }
            }

            float radians = Mathf.Deg2Rad * angle;

            Vector3 position = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
            Debug.Log(position);
            position = transform.position + new Vector3(position.x, position.y * height, position.z);
            list[i].handler.SetPosition(position);

            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle * height);
            list[i].handler.SetQuaternion(targetRotation);

            list[i].handler.DoMove();
        }
    }
}
