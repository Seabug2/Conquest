using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Hand : NetworkBehaviour
{
    [SerializeField, Header("��ȣ")] int seatNum;
    public int SeatNum => seatNum;

    //[SerializeField, Header("����Ʈ")] 
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

    #region �� ����
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

    #region �߰�, ����
    [Command(requiresAuthority = false)]
    public void CmdAdd(int id)
    {
        RpcAdd(id);
    }

    [ClientRpc]
    public void RpcAdd(int id)
    {
        Add(id);
    }

    [Client]
    public void Add(int id)
    {
        Card newCard = GameManager.Card(id);
        newCard.ownerOrder = seatNum;

        newCard.iCardState
            = GameManager.GetPlayer(seatNum).isLocalPlayer
            ? new InHandState(newCard, this)
            : GameManager.instance.noneState;

        newCard.IsOpened = GameManager.GetPlayer(seatNum).isLocalPlayer;

        cards.Add(newCard);
        HandAlignment();
    }

    [Client]
    public void Add(Card newCard)
    {
        newCard.transform.position = transform.position + Vector3.right * 6f;
        newCard.ownerOrder = seatNum;
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
        Remove(id);
    }

    [Client]
    public void Remove(int id)
    {
        Card discard = GameManager.Card(id);
        cards.Remove(discard);

        if (cards.Count > 0)
            HandAlignment();
    }

    [Client]
    public void Remove(Card discard)
    {
        cards.Remove(discard);

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
        RemoveAll();
    }

    [Client]
    public void RemoveAll()
    {
        cards.Clear();
    }


    [Client]
    public void RandomDiscard()
    {
        if (cards.Count < 1) return;

        int rand = Random.Range(0, cards.Count);
        Card discard = cards[rand];
        cards.Remove(discard);
        discard.SetTargetPosition(new Vector3(0, 0, 0));
        discard.DoMove(() => Destroy(discard.gameObject), 0.1f);
        if (cards.Count > 0)
        {
            HandAlignment();
        }
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
    [Range(1f, 10f), Header("�ʺ�")]
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
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        float halfAngleRange = Mathf.Clamp(intervalAngle * cardCount * .5f, 0, maxAngle); //�� ���� �ִ밢�� ���Ѵ�
        bool isAngleClamped = halfAngleRange == maxAngle;

        float leftEndAngle = -halfAngleRange;
        float rightEndAngle = halfAngleRange;
        float interval = isAngleClamped ? 1f / (cardCount - 1) : 1f / (cardCount + 1); //lerp�� ������ ����

        //���� ���콺�� �÷��� ī�尡 �ִ��� Ȯ��
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

            //������ ī�尡 �ִ� ���
            if (selectedCardIndex >= 0)
            {
                AdjustForSelectedCard(ref position, ref rotation, i, selectedCardIndex, cardCount);
            }

            card.SetTargetPosition(position);
            card.SetTargetQuaternion(rotation);
            card.DoMove();
        }
    }

    /// <summary>
    /// �־��� ������ �������� �ٲٰ� �ﰢ�Լ��� ����Ͽ� �������κ����� ��ġ�� ���
    /// </summary>
    private Vector3 CalculateCardPosition(float angle)
    {
        float radians = Mathf.Deg2Rad * angle;
        Vector3 offset = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
        return transform.position + new Vector3(offset.x, offset.y * height, 0);
    }

    //
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

    public void StateToggle()
    {
        if (Count == 0) return;

        bool isHandlingState = (cards[0].iCardState.GetType().Equals(typeof(HandlingState)));
        Debug.Log("Change to " + (isHandlingState ? "InHandState" : "HandlingState"));

        foreach (Card card in cards)
        {
            card.iCardState = isHandlingState
                ? new InHandState(card, this)
                : new HandlingState(card, this, null);

            card.IsOnMouse = false;
            card.SprtRend.sortingLayerName = "Default";
            card.transform.localScale = Vector3.one;
        }
        HandAlignment();
    }
}
