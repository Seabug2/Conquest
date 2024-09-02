using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;


public class Token : MonoBehaviour
{
    [SerializeField] int id;
    public int ID => id;

    //카드의 이름
    [SerializeField] string villainName;
    public string VillainName => villainName;

    //카드 설명
    [SerializeField] string flavorText;
    public string FlavorText => flavorText;

    //카드 효과
    [SerializeField] string abilityText;
    public string AbilityText => abilityText;

    /// <summary>
    /// 카드를 전개 했을 때, 혹은 효과를 발동할 때 나올 컷씬
    /// </summary>
    [SerializeField] GameObject eventScene;

    public bool IsOpened { get; private set; }

    //소켓에 대한 정보
    public Socket[] Sockets { get; private set; }

    private void Start()
    {
        Sockets = GetComponentsInChildren<Socket>();
        IsOpened = false;
    }

    /// <summary>
    /// 카드가 전개 되었을 때 호출
    /// </summary>
    public virtual void OnLinked()
    {
        //eventScene?.SetActive(true);
    }

    /// <summary>
    /// 카드가 덱으로 되돌아 갈 때 호출
    /// </summary>
    public virtual void OnReturnToDeck()
    {
        
    }

    /// <summary>
    /// 카드가 필드에서 제거될 때
    /// 더 이상 효과를 발동하지 않을 때
    /// </summary>
    public virtual void OnFieldOut()
    {
        
    }
    
    //이 카드를 전개 했을 때 전개한 카드와, 모든 소켓에 연결된 카드에서 실행하여
    //완성된 카드가 있는지 확인하고 있다면 효과와 덱으로 되돌리는 효과를 실행
    /// <summary>
    /// 모든 소켓을 연결했는지 확인하는 메서드
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
    /// 카드를 전개했을 때
    /// </summary>
    protected virtual void OnComplete()
    {
        
    } 
}
