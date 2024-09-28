using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Hand : NetworkBehaviour
{
    [SerializeField, Header("��ȣ")] int seatNum;
    public int SeatNum => seatNum;

    private void Start()
    {
        GameManager.dict_Hand.Add(seatNum, this);
    }
    [Space(10)]
    [SerializeField, Header("����Ʈ")] List<Card> list = new();

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

    #region �� ����
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

    #region �߰�, ����
    [Command(requiresAuthority = false)]
    public void CmdAdd(int id)
    {
        RpcAdd(id);
    }

    [ClientRpc]
    public void RpcAdd(int id)
    {
        Card newCard = GameManager.Card(id);
        newCard.ownerOrder = seatNum;

        if (GameManager.GetPlayer(seatNum).isLocalPlayer)
        {
            newCard.iCardState = new InHandState(newCard, this);
            newCard.IsOpened = true;
        }
        else
        {
            newCard.iCardState = GameManager.instance.noneState;
            newCard.IsOpened = false;
        }

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
        drawnCard.iCardState = GameManager.instance.noneState;
        list.Remove(drawnCard);

        if (list.Count > 0)
            HandAlignment();
    }
    #endregion

    public void HandOpen(bool isOpen)
    {
        foreach (Card c in list)
        {
            c.IsOpened = isOpen;
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

    [Range(1f, 10f),Header("�ʺ�")]
    public float radius;
    [Range(0f, 1f), Header("ȣ ����")]
    public float height;
    [Range(0f, 90), Header("ī�� ���� ����")]
    //ī�� ����
    public float intervalAngle;
    //�ִ� ī�� ��
    public float maxAngle;
    [Header("���õ� ī���� ���� ������")]
    public Transform selectedCardHeight;

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
        int count = list.Count;

        float rightEndAngle = Mathf.Clamp(intervalAngle * count * .5f, 0, maxAngle); //�� ���� �ִ밢�� ���Ѵ�
        bool isOver = rightEndAngle == maxAngle;

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

            if (list[i].IsOnMouse) continue;

            float angle = Mathf.Lerp(leftEndAngle, rightEndAngle, interval * (i + (isOver ? 0 : 1)));

            float radians = Mathf.Deg2Rad * angle;

            Vector3 position = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
            position = transform.position + new Vector3(position.x, position.y * height, 0);

            Quaternion targetRotation;

            //������ ī�尡 �ִ� ���
            if (selectedNum != -1)
            {
                if (i < selectedNum)
                {
                    float t = 1f / selectedNum;
                    position.x -= 0.8f;
                    position.y += Mathf.Lerp(0, height, t * i);
                    targetRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(leftEndAngle, 0, t * i) * height);
                }
                else
                {
                    float t = 1f / (count - 1 - selectedNum);
                    position.x += 0.8f;
                    position.y += Mathf.Lerp(height, 0, t * (i - selectedNum));
                    targetRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(0, rightEndAngle, t * (i - selectedNum)) * height);
                }
            }
            //������ ī�尡 ���� ���
            else
            {
                targetRotation = Quaternion.Euler(0, 0, -angle * height);
            }

            list[i].SetTargetPosition(position);
            list[i].SetTargetQuaternion(targetRotation);

            list[i].DoMove();
        }
    }

    public bool HasValidCards(Field _field)
    {
        if (list.Count < 1) return false;

        foreach(Card c in list)
        {
            //_field. (c);....
        }

        return true;
    }

    public void SetHandlingState(Field _myField = null)
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
