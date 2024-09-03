using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;


public class Piece : MonoBehaviour
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

    public virtual void OnLinked()
    {

    }

    /// <summary>
    /// ī�尡 ������ �ǵ��� �� �� ȣ��
    /// </summary>
    public virtual void OnReturnToDeck()
    {
        
    }

    /// <summary>
    /// ī�尡 �ʵ忡�� ���ŵ� ��
    /// �� �̻� ȿ���� �ߵ����� ���� ��
    /// </summary>
    public virtual void OnFieldOut()
    {
        
    }
    
    //�� ī�带 ���� ���� �� ������ ī���, ��� ���Ͽ� ����� ī�忡�� �����Ͽ�
    //�ϼ��� ī�尡 �ִ��� Ȯ���ϰ� �ִٸ� ȿ���� ������ �ǵ����� ȿ���� ����
    /// <summary>
    /// ��� ������ �����ߴ��� Ȯ���ϴ� �޼���
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
