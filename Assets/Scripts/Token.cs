using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


public class Token : MonoBehaviour
{
    [SerializeField] int id;
    public int ID => id;

    //ī���� �̸�
    [SerializeField] string villainName;
    public string VillainName => villainName;

    //ī�� ����
    [SerializeField] string flavorText;
    public string FlavorText => flavorText;

    //ī�� ȿ��
    [SerializeField] string abilityText;
    public string AbilityText => abilityText;

    /// <summary>
    /// ī�带 ���� ���� ��, Ȥ�� ȿ���� �ߵ��� �� ���� �ƾ�
    /// </summary>
    [SerializeField] GameObject eventScene;

    public bool IsOpened { get; private set; }

    //���Ͽ� ���� ����
    public Socket[] Sockets { get; private set; }

    private void Start()
    {
        Sockets = GetComponentsInChildren<Socket>();
        IsOpened = false;
    }

    /// <summary>
    /// ī�尡 ���� �Ǿ��� �� ȣ��
    /// </summary>
    public virtual void OnLinked()
    {
        //eventScene?.SetActive(true);
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

        foreach(Socket s in Sockets)
        {
            if (!s.isFilled)
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
