using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkIdentity))]
public class Hand : NetworkBehaviour
{
    [SerializeField] int seatNum;
    public int SeatNum => seatNum;

    /// <summary>
    /// �п� �ִ� ī���
    /// </summary>
    [SerializeField] List<Card> list = new List<Card>();

    /// <summary>
    /// �տ� �ִ� ��� ī���� ID�� ��ȯ�մϴ�.
    /// </summary>
    public int[] AllIDs
    {
        get
        {
            int[] ids = new int[Count];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = list[i].id;
            }

            return ids;
        }
    }

    public int Count => list.Count;

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

    #region �߰�, ����
    public void Add(int id)
    {
        Add(GameManager.Card(id));
    }

    public void Add(Card newCard)
    {
        if (SeatNum.Equals(GameManager.LocalPlayer.order))
        {
            newCard.iCardState = new InHandState(newCard, this);
            newCard.IsOpened = true;
        }
        else
        {
            newCard.iCardState = new NoneState();
            newCard.IsOpened = false;
        }
        newCard.ownerOrder = seatNum;
        list.Add(newCard);
        HandAlignment();
    }


    public void TestAdd(Card newCard)
    {
        newCard.IsOpened = true;
        newCard.iCardState = new InHandState(newCard, this);
        list.Add(newCard);
        HandAlignment();
    }
    public void TestChangeState()
    {
        foreach (Card c in list)
        {
            c.iCardState = new InHandOnTurn(c, this, null);
        }
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
        drawnCard.iCardState = new NoneState();
        list.Remove(drawnCard);

        if (list.Count > 0)
            HandAlignment();
    }
    #endregion

    public void HandOpen()
    {
        foreach (Card c in list)
        {
            c.IsOpened = !c.IsOpened;
            if (c.iCardState.GetType().Equals(typeof(InHandOnTurn)))
            {
                c.iCardState = new InHandOnTurn(c, this, null);
            }
            else if (c.iCardState.GetType().Equals(typeof(InHandState)))
            {
                c.iCardState = new InHandState(c, this);
            }
        }
    }

    [Range(1f, 10f)]
    public float radius;
    [Range(0f, 1f)]
    public float height;
    [Range(0f, 90)]
    //ī�� ����
    public float intervalAngle;
    //�ִ� ī�� ��
    public float maxAngle;

    public void HandAlignment()
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
            if (list[i].IsOnMouse)
            {
                selectedNum = i;
                break;
            }
        }

        for (int i = 0; i < count; i++)
        {
            list[i].SprtRend.sortingOrder = 1 + i; //ī���� Sorting Order�� i������ �Ҵ�
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
            list[i].SetTargetPosition(position);

            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle * height);
            list[i].SetTargetQuaternion(targetRotation);

            list[i].DoMove(0, DG.Tweening.Ease.Unset);
            //list[i].StartTween();
        }
    }

    public void HandSetting(Field _myField)
    {
        if (_myField != null)
        {
            foreach (Card card in list)
            {
                card.iCardState = new InHandOnTurn(card, this, _myField);
            }
        }
        else
        {
            foreach (Card card in list)
            {
                card.iCardState = new InHandState(card, this);
            }
        }
    }
}
