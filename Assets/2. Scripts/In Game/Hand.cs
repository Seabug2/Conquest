using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Hand : NetworkBehaviour
{
    [SerializeField] int seatNum;
    public int SeatNum => seatNum;

    private void Start()
    {
        GameManager.dict_Hand.Add(seatNum, this);
    }

    [SerializeField] List<Card> list = new();

    public int Count => list.Count;

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

    #region 패 제한
    const int handsLimit = 6;

    public int LimitStack { get; private set; }

    public void SetLimitStack(int i)
    {
        LimitStack += i;
        if (LimitStack < 0) LimitStack = 0;
    }

    public int HandsLimit => handsLimit + LimitStack;

    public bool IsLimitOver => list.Count > HandsLimit;
    #endregion

    #region 추가, 제거
    [Command(requiresAuthority = false)]
    public void CmdAdd(int id)
    {
        Add(id);
    }

    [ClientRpc]
    public void Add(int id)
    {
        Card newCard = GameManager.Card(id);

        if (GameManager.GetPlayer(seatNum).isLocalPlayer)
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
            c.iCardState = new HandlingState(c, this, null);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRemove(int id)
    {
        RpcRemove(id);
    }

    [ClientRpc]
    public void RpcRemove(int id)
    {
        Card drawnCard = GameManager.Card(id);
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
            if (c.iCardState.GetType().Equals(typeof(HandlingState)))
            {
                c.iCardState = new HandlingState(c, this, null);
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

    [Command(requiresAuthority = false)]
    public void CmdHandAlignment()
    {
        RpcHandAlignment();
    }

    [ClientRpc]
    public void RpcHandAlignment()
    {
        HandAlignment();
    }

    [Client]
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

            if (list[i].IsOnMouse) continue;

            float angle = Mathf.Lerp(leftEndAngle, rightEndAngle, interval * (i + (isOver ? 0 : 1)));

            /*
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
                    angle = Mathf.Lerp(leftEndAngle, selectedAngle, (float)i / selectedNum);
                }
                else if (i > selectedNum)
                {
                    angle = Mathf.Lerp(selectedAngle, rightEndAngle, (float)(i - selectedNum) / (count - 1 - selectedNum));
                }
                else
                {
                    continue;
                }
            }
            */

            float radians = Mathf.Deg2Rad * angle;

            Vector3 position = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
            position = transform.position + new Vector3(position.x, position.y * height, position.z);

            Quaternion targetRotation;

            //선택한 카드가 있는 경우
            if (selectedNum != -1)
            {
                position += Vector3.right * (i > selectedNum ? 0.9f : -0.9f);

                if (i < selectedNum)
                {
                    float t = 1f / selectedNum;
                    targetRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(leftEndAngle, 0, t * i) * height);
                }
                else
                {
                    float t = 1f / (count - 1 - selectedNum);
                    targetRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(0, rightEndAngle, t * (i - selectedNum)) * height);
                }
            }
            //선택한 카드가 없는 경우
            else
            {
                targetRotation = Quaternion.Euler(0, 0, -angle * height);
            }




            list[i].SetTargetPosition(position);
            list[i].SetTargetQuaternion(targetRotation);

            list[i].DoMove();
        }
    }

    public void HandSetting(Field _myField = null)
    {
        if (_myField == null)
        {
            foreach (Card card in list)
            {
                card.iCardState = new InHandState(card, this);
            }
        }
        else
        {
            foreach (Card card in list)
            {
                card.iCardState = new HandlingState(card, this, _myField);
            }
        }
    }
}
