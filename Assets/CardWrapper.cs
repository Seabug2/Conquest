using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardWrapper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    /*
     1. 카드 위에 마우스를 올렸을 때, 카드가 공개된 상태라면 해당 카드의 정보가 UI Image 로 나타난다.
     2. 마우스가 카드를 벗어나면 활성화 됐던 UI가 다시 비활성화 된다.
     3. 카드가 필드에 있지 않은 상태라면 카드는 마우스 위치로 이동함
     */
    public bool isOpend = false;

    public bool isSelectable = false;

    /// <summary>
    /// 현재 선택 중인 상태인지를 나타냅니다.
    /// </summary>
    bool isSelecting = false;
    bool isOnField = false;


    public Vector3 myTargetPosition;

    [SerializeField, Tooltip("마우스를 올려뒀을 때 카드의 크기")] Vector3 onPointerScale;
    [SerializeField, Tooltip("마우스를 올려뒀을 때 카드의 정보를 보여줄 UI")] GameObject popupImg;

    Camera cam;
    
    private void Start()
    {
        cam = Camera.main;
        popupImg.SetActive(false);
        myTargetPosition = transform.position;
    }

    public float speed = 5f;

    private void FixedUpdate()
    {
        if (!isSelecting)
            transform.position = Vector3.Lerp(transform.position, myTargetPosition, Time.fixedDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isOpend)
        {
            popupImg.SetActive(isOpend);
        }

        //카드가 패에 있고 선택 가능한 상태일 경우
        transform.localScale = onPointerScale;

    }



    public void OnPointerExit(PointerEventData eventData)
    {
        //원래의 크기로 되돌린다.
        transform.localScale = Vector3.one;
        isSelecting = false;
        popupImg.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSelecting) return;

        //카드가 마우스 포인터를 따라 다녀야함
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //카드를 어떤 타일 위에 드래그 앤 드랍한 순간일 경우
        //그 타일 위치에 카드를 전개할 수 있다.
    }
}
