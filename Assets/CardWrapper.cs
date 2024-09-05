using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardWrapper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    /*
     1. ī�� ���� ���콺�� �÷��� ��, ī�尡 ������ ���¶�� �ش� ī���� ������ UI Image �� ��Ÿ����.
     2. ���콺�� ī�带 ����� Ȱ��ȭ �ƴ� UI�� �ٽ� ��Ȱ��ȭ �ȴ�.
     3. ī�尡 �ʵ忡 ���� ���� ���¶�� ī��� ���콺 ��ġ�� �̵���
     */
    public bool isOpend = false;

    public bool isSelectable = false;

    /// <summary>
    /// ���� ���� ���� ���������� ��Ÿ���ϴ�.
    /// </summary>
    bool isSelecting = false;
    bool isOnField = false;


    public Vector3 myTargetPosition;

    [SerializeField, Tooltip("���콺�� �÷����� �� ī���� ũ��")] Vector3 onPointerScale;
    [SerializeField, Tooltip("���콺�� �÷����� �� ī���� ������ ������ UI")] GameObject popupImg;

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

        //ī�尡 �п� �ְ� ���� ������ ������ ���
        transform.localScale = onPointerScale;

    }



    public void OnPointerExit(PointerEventData eventData)
    {
        //������ ũ��� �ǵ�����.
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

        //ī�尡 ���콺 �����͸� ���� �ٳ����
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //ī�带 � Ÿ�� ���� �巡�� �� ����� ������ ���
        //�� Ÿ�� ��ġ�� ī�带 ������ �� �ִ�.
    }
}
