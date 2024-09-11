using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour
{
    public int seatNum;

    [SerializeField] List<Card> list = new List<Card>();

    /// <summary>
    /// 손에 있는 모든 카드의 ID를 반환합니다.
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

    public Card this[int index]
    {
        get
        {
            if (index < 0 || index >= list.Count)
            {
                Debug.Log($"Index 범위 오류");
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
    /// ClientRpc로 호출 될 때
    /// isLoclaPlayer == true 일 경우,
    /// 반드시 해당 id의 카드는 handler를 활성화 시켜줘야 한다.
    /// </summary>
    #region 추가, 제거
    public void Add(int id)
    {
        Add(GameManager.Card(id));
    }

    public void Add(Card drawnCard)
    {
        list.Add(drawnCard);
        drawnCard.handler.OnMouseAction = HandAlignment;
        HandAlignment();
        //카드를...
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
        list.Remove(drawnCard);

        if (list.Count > 0)
            HandAlignment();
    }
    #endregion

    public float radius = 1;
    public float height = 1;
    //카드 간격
    public float intervalAngle = 18f;
    //최대 카드 각
    public float maxAngle = 54;

    void HandAlignment()
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
            if (list[i].handler.IsOnMouse)
            {
                selectedNum = i;
                break;
            }
        }

        for (int i = 0; i < count; i++)
        {
            list[i].sprtRend.sortingOrder = 1 + i; //카드의 Sorting Order를 i순으로 할당
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
            list[i].handler.SetPosition(position);

            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle * height);
            list[i].handler.SetQuaternion(targetRotation);

            list[i].handler.DoMove();
        }
    }
}
