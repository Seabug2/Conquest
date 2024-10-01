using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Hand : NetworkBehaviour
{
    [SerializeField, Header("번호")] int seatNum;
    public int SeatNum => seatNum;

    private void Start()
    {
        GameManager.dict_Hand.Add(seatNum, this);
    }

    //[SerializeField, Header("리스트")] 
    readonly List<Card> cards = new();

    public int Count => cards.Count;

    public int[] AllIDs
    {
        get
        {
            int[] ids = new int[Count];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = cards[i].id;
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

    public bool IsLimitOver => cards.Count > HandsLimit;
    #endregion

    #region 추가, 제거
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

        cards.Add(newCard);
        HandAlignment();
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
        cards.Remove(drawnCard);

        if (cards.Count > 0)
            HandAlignment();
    }

    [Command(requiresAuthority = false)]
    public void CmdRemoveAll()
    {
        RpcRemoveAll();
    }

    [ClientRpc]
    public void RpcRemoveAll()
    {
        cards.Clear();
    }


    #endregion

    public void HandOpen(bool isOpen)
    {
        foreach (Card c in cards)
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

    [Space(10)]
    [Range(1f, 10f), Header("너비")]
    public float radius;
    [Range(0f, 1f), Header("호 높이")]
    public float height;
    [Range(0f, 90), Header("카드 간격 각도")]
    //카드 간격
    public float intervalAngle;
    //최대 카드 각
    public float maxAngle;
    [Header("선택된 카드의 높이 참조점")]
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
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        float halfAngleRange = Mathf.Clamp(intervalAngle * cardCount * .5f, 0, maxAngle); //한 쪽의 최대각을 구한다
        bool isAngleClamped = halfAngleRange == maxAngle;

        float leftEndAngle = -halfAngleRange;
        float rightEndAngle = halfAngleRange;
        float interval = isAngleClamped ? 1f / (cardCount - 1) : 1f / (cardCount + 1); //lerp의 간격을 설정

        //현재 마우스를 올려둔 카드가 있는지 확인
        int selectedCardIndex = cards.FindIndex(card => card.IsOnMouse);

        for (int i = 0; i < cardCount; i++)
        {
            Card card = cards[i];
            card.SprtRend.sortingOrder = i + 1;

            if (card.IsOnMouse) continue;

            float t = interval * (i + (isAngleClamped ? 0 : 1));
            float angle = Mathf.Lerp(leftEndAngle, rightEndAngle, t);


            Vector3 position = CalculateCardPosition(angle);
            Quaternion rotation = Quaternion.Euler(0, 0, -angle * height);

            //선택한 카드가 있는 경우
            if (selectedCardIndex >= 0)
            {
                AdjustForSelectedCard(ref position, ref rotation, i, selectedCardIndex, cardCount);
            }

            card.SetTargetPosition(position);
            card.SetTargetQuaternion(rotation);
            card.DoMove();
        }
    }

    private Vector3 CalculateCardPosition(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        Vector3 offset = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
        return transform.position + new Vector3(offset.x, offset.y * height, 0);
    }

    private void AdjustForSelectedCard(ref Vector3 position, ref Quaternion rotation, int index, int selectedIndex, int totalCards)
    {
        if (index < selectedIndex)
        {
            float t = 1f / selectedIndex;
            position.x -= 0.8f;
            position.y += Mathf.Lerp(0, height, t * index);
            float adjustedAngle = Mathf.Lerp(-maxAngle, 0, t * index) * height;
            rotation = Quaternion.Euler(0, 0, -adjustedAngle);
        }
        else if (index > selectedIndex)
        {
            float t = 1f / (totalCards - selectedIndex - 1);
            position.x += 0.8f;
            position.y += Mathf.Lerp(height, 0, t * (index - selectedIndex));
            float adjustedAngle = Mathf.Lerp(0, maxAngle, t * (index - selectedIndex)) * height;
            rotation = Quaternion.Euler(0, 0, -adjustedAngle);
        }
    }

    public void SetHandlingState(Field _myField = null)
    {
        if (Count == 0) return;

        foreach (Card card in cards)
        {
            card.iCardState = _myField == null
                ? new InHandState(card, this)
                : new HandlingState(card, this, _myField);
            card.IsOnMouse = false;
            card.SprtRend.sortingLayerName = "Default";
            card.transform.localScale = Vector3.one;
        }
        HandAlignment();
    }
}
