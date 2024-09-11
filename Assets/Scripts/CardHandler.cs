using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

[RequireComponent(typeof(Card))]
public class CardHandler : NetworkBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    ///나의 차례가 되었을 때 덱에서 카드를 드로우 하고 난 후, true가 된다.
    /// </summary>
    public bool isSelectable = false;
    public bool IsOnMouse { get; private set; }

    Camera cam;

    //private void Awake()
    //{
    //    IsOnMouse = false;
    //    cam = Camera.main;
    //}

    private void Start()
    {
        IsOnMouse = false;
        cam = Camera.main;
    }

    #region Drag Event
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSelectable) return;

        //현재 트윈이 실행 중이라면 일단 중단
        transform.DOKill();
        //mousePositionOffset = 
    }

    Vector3 mousePositionOffset;

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSelectable) return;
        //나의 화면에서만 마우스를 따라 이동할 예정
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isSelectable) return;
        //드래그를 마쳤을 때 마우스 위치에서 Raycast를 하여
        //검출된 타일이 있는 경우 그 타일 위치로 이동...
    }
    #endregion



    [Command]
    void CmdSetTargetPosition(float x, float y, float z)
    {
        RpcSetTargetPosition(x, y, z);
    }

    [ClientRpc]
    void RpcSetTargetPosition(float x, float y, float z)
    {
        SetPosition(x, y, z);
    }



    [Command]
    public void CmdDoMove(float delay)
    {
        RpcDoMove(delay);
    }

    [ClientRpc]
    void RpcDoMove(float delay)
    {
        DoMove(delay);
    }
    [Command]
    public void CmdDoMove()
    {
        RpcDoMove();
    }

    [ClientRpc]
    void RpcDoMove()
    {
        DoMove();
    }




    [HideInInspector]
    public Vector3 targetPosition { get; private set; }

    public void SetPosition(float x, float y, float z)
        => targetPosition.Set(x, y, z);

    public void SetPosition(Vector3 targetPosition)
        => this.targetPosition = targetPosition;



    [HideInInspector]
    public Quaternion targetRotation { get; private set; }
    public void SetQuaternion(float x, float y, float z, float w)
        => targetRotation.Set(x, y, z, w);

    public void SetQuaternion(Quaternion targetRotation)
        => this.targetRotation = targetRotation;



    const float duration = 0.5f;

    public void DoMove(float delay = 0, Ease setEase = Ease.InOutQuint)
    {
        transform.DOKill();
        transform.DOMove(targetPosition, duration).SetEase(setEase).SetDelay(delay);
        transform.DORotateQuaternion(targetRotation, duration).SetEase(setEase).SetDelay(delay);
    }






    public Action OnMouseAction;

    //모든 플레이어의 화면에서 위치 이동
    #region OnPointerEnter
    public void OnPointerEnter(PointerEventData eventData)
    {
        CmdOnPointerEnter();
    }

    public float upPositionOffest = 0;

    [Command(requiresAuthority = false)]
    void CmdOnPointerEnter()
    {

        RpcOnPointerEnter();
    }

    [ClientRpc]
    void RpcOnPointerEnter()
    {
        IsOnMouse = true;
        Vector3 OnMousePosition = targetPosition + Vector3.up * upPositionOffest;
        Debug.Log($"RpcOnPointerEnter : {OnMousePosition }");

        SetPosition(OnMousePosition);
        targetRotation = Quaternion.identity;
        DoMove();

        OnMouseAction?.Invoke();
    }
    #endregion

    #region OnPointerExit
    public void OnPointerExit(PointerEventData eventData)
    {
        //카드를 필드에 낼 수 있는 경우,
        //EndDrag 이벤트로만 원위치로 이동할 수 있다.
        if (isSelectable) return;
        CmdOnPointerExit();
    }

    [Command(requiresAuthority = false)]
    void CmdOnPointerExit()
    {
        RpcOnPointerExit();
    }

    [ClientRpc]
    void RpcOnPointerExit()
    {
        IsOnMouse = false;
        OnMouseAction?.Invoke();
    }
    #endregion
}