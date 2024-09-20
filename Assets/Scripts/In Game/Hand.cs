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
    /// 패에 있는 카드들
    /// </summary>
    [SerializeField] List<Card> list = new List<Card>();

    /// <summary>
    /// 손에 있는 모든 카드의 ID를 반환합니다.
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

    //기본 손 패 제한
    const int handsLimit = 6;

    public int LimitStack { get; private set; }

    public void SetLimitStack(int i)
    {
        LimitStack += i;
        if (LimitStack < 0) LimitStack = 0;
    }

    public int HandsLimit => handsLimit + LimitStack;

    public bool IsLimitOver => list.Count > HandsLimit;

    #region 추가, 제거
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
    /// ClientRpc로 호출 될 때
    /// 반드시 해당 id의 카드는 handler를 비활성화 시켜줘야 한다.
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
    //카드 간격
    public float intervalAngle;
    //최대 카드 각
    public float maxAngle;

    public void HandAlignment()
    {
        float count = list.Count;
        bool isOver = false; //최대 각도로 카드를 벌렸는지 확인하는 bool 값
        float rightEndAngle = intervalAngle * count * .5f; //한 쪽의 최대각을 구한다
        if (rightEndAngle > maxAngle)
        {
            rightEndAngle = maxAngle;
            isOver = true;
        }
        float leftEndAngle = -rightEndAngle; //반대 쪽의 최대각을 할당

        float interval = isOver ? 1f / (count - 1) : 1f / (count + 1); //lerp의 간격을 설정

        //현재 마우스를 올려둔 카드가 있는지 확인
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
            list[i].SprtRend.sortingOrder = 1 + i; //카드의 Sorting Order를 i순으로 할당
            float angle;

            if (selectedNum < 0 || selectedNum >= count) //마우스를 올려둔 카드가 없을 때
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
