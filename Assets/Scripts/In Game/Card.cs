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
    /// ī�带 ���� ���� ��, Ȥ�� ȿ���� �ߵ��� �� ���� �ƾ�
    /// </summary>
    [SerializeField] GameObject eventScene;

    //���Ͽ� ���� ����
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
    /// ī�尡 ���� �Ǿ��� �� ȣ��
    /// </summary>
    public virtual void OnDeployed()
    {
        //eventScene?.SetActive(true);
    }


    /// <summary>
    /// �ٸ� ���� ������ ���� ��ũ�Ǿ��� ��
    /// </summary>
    public virtual void OnLinked()
    {

    }

    /// <summary>
    /// ������ �ǵ��� �� �� ȣ��
    /// </summary>
    public virtual void OnReturnToDeck()
    {

    }

    /// <summary>
    /// �ʵ忡�� ���ŵ� ��
    /// �� �̻� ȿ���� �ߵ����� ���� ��
    /// </summary>
    public virtual void OnFieldOut()
    {

    }

    /// <summary>
    /// ��� ������ ��ũ �Ǿ����� Ȯ���ϴ� �޼���
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
    /// ��ũ�� �ϼ����� ��
    /// </summary>
    protected virtual void OnComplete()
    {

    }

    public PositionKeeper positionKeeper { get; private set; }
}
