using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] int id;
    public int ID => id;

    /// <summary>
    /// 카드를 전개 했을 때, 혹은 효과를 발동할 때 나올 컷씬
    /// </summary>
    [SerializeField] GameObject eventScene;

    //소켓에 대한 정보
    // 0   1
    //
    // 3   2
    [SerializeField] Socket[] sockets;
    public Socket[] Sockets => sockets;

    public CardHandler handler;

    private void Start()
    {
        if (!TryGetComponent(out handler))
        {
            handler = gameObject.AddComponent<CardHandler>();
        }
    }

    /// <summary>
    /// 카드가 전개 되었을 때 호출
    /// </summary>
    public virtual void OnDeployed()
    {
        //eventScene?.SetActive(true);
    }


    /// <summary>
    /// 다른 빌런 유닛이 새로 링크되었을 때
    /// </summary>
    public virtual void OnLinked()
    {

    }

    /// <summary>
    /// 덱으로 되돌아 갈 때 호출
    /// </summary>
    public virtual void OnReturnToDeck()
    {

    }

    /// <summary>
    /// 필드에서 제거될 때
    /// 더 이상 효과를 발동하지 않을 때
    /// </summary>
    public virtual void OnFieldOut()
    {

    }

    /// <summary>
    /// 모든 소켓이 링크 되었는지 확인하는 메서드
    /// </summary>
    public void ConnectionCheck()
    {
        bool isCompleted = true;

        foreach (Socket s in sockets)
        {
            if (!s.IsFilled)
            {
                isCompleted = false;
                break;
            }
        }

        if (isCompleted)
        {
            OnComplete();
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// 링크를 완성했을 때
    /// </summary>
    protected virtual void OnComplete()
    {

    }

    public PositionKeeper positionKeeper { get; private set; }
}
