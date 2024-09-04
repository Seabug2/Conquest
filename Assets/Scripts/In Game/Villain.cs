using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Villain : MonoBehaviour
{
    [SerializeField] int id;
    public int ID => id;

    /// <summary>
    /// ī�带 ���� ���� ��, Ȥ�� ȿ���� �ߵ��� �� ���� �ƾ�
    /// </summary>
    [SerializeField] GameObject eventScene;

    public bool IsOpened { get; private set; }

    //���Ͽ� ���� ����
    // 0   1
    //
    // 3   2
    [SerializeField]Socket[] sockets;
    public Socket[] Sockets => sockets;

    private void Start()
    {
        //Sockets = GetComponentsInChildren<Socket>();
        IsOpened = false;
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

        foreach(Socket s in sockets)
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
    /// ī�带 �������� ��
    /// </summary>
    protected virtual void OnComplete()
    {
        
    } 
}
