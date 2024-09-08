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

    public void SetLimitStack(int i) => LimitStack += i;

    public int HandsLimit() => handsLimit + LimitStack;

    public bool IsLimitOver() => list.Count > HandsLimit();

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

    #region 추가, 제거
    public void Add(int id)
    {
        Add(GameManager.Card(id));
    }

    public void Add(Card drawnCard)
    {
        list.Add(drawnCard);

        UpdateHand();
        //카드를...
    }


    public void Remove(int id)
    {
        Remove(GameManager.Card(id));
    }

    public void Remove(Card drawnCard)
    {
        list.Remove(drawnCard);

        if (list.Count > 0)
            UpdateHand();
    }
    #endregion

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateHand();
        }
    }

    public void UpdateHand()
    {
        int handCount = list.Count;

        if (handCount > 6)
        {
            float t = 1 / handCount;

            for (int i = 0; i < handCount; i++)
            {
                float normalizedValue = Mathf.InverseLerp(0, handCount - 1, i);
                float x = Mathf.Lerp(-3f, 3f, normalizedValue);
                list[i].handler.SetPosition(transform.position.x + x, transform.position.y, transform.position.z);
            }
        }
        else
        {
            for (int i = 0; i < handCount; i++)
            {
                float x = -0.5f * (list.Count - 1 - i) + 0.5f * i;

                list[i].handler.SetPosition(transform.position.x + x, transform.position.y, transform.position.z);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 1));
    }
}
