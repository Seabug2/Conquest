using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

[RequireComponent(typeof(NetworkIdentity))]
public class CardHandler : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 카드가 앞면이라면 마우스를 위에 올렸을 때 카드 정보 UI를 활성화한다.
    /// </summary>
    bool isOpened = false;
    public bool IsOpened
    {
        get
        {
            return isOpened;
        }
        set
        {
            isOpened = value;
            if (isOpened)
                GetComponent<SpriteRenderer>().sprite = front;
            else
                GetComponent<SpriteRenderer>().sprite = GameManager.instance.cardBackFace;
        }

    }
    Sprite front;

    NetworkPlayer owner;
    public void SetOwner(NetworkPlayer owner)
    {
        this.owner = owner;
    }
    public NetworkPlayer Owner => owner;

    [Header("카드의 상세정보 UI"), Tooltip("앞 면으로 공개된 카드는 마우스를 올려두면 카드 정보 UI가 나타납니다.")]
    public GameObject info;

    bool isSelectable = false;

    public void SerSelectable()
    {
        isSelectable = true;
    }


    Camera cam;
    Tween tween;

    private void Awake()
    {
        front = GetComponent<SpriteRenderer>().sprite;
        cam = Camera.main;
    }

    bool isMine;

    #region Drag Event
    public void OnBeginDrag(PointerEventData eventData)
    {
        //만약 tween이 실행 중이라면...
        if (isSelectable)
        {
            //드래그를 종료할 때 마우스의 위치에 타일이 있다면
            //그 타일을 검사한다.
            

            if (tween.IsPlaying())
            {
                tween.Kill();
            }

            if (info.activeSelf)
            {
                info.SetActive(false);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //tween을 중지하고 마우스를 따라다니도록 한다.
        if (isSelectable)
        {
            Vector3 screenPosition = new Vector3(eventData.position.x, eventData.position.y, cam.WorldToScreenPoint(transform.position).z);
            transform.position = cam.ScreenToWorldPoint(screenPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DoMove();
    }
    #endregion



    Vector3 position;
    public Vector3 targetRotation;

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

    public void SetPosition(float x, float y, float z, float delay = 0)
    {
        position.Set(x, y, z);
        DoMove(delay);
    }

    public void SetPosition(Vector3 targetPosition, float delay = 0)
    {
        position = targetPosition;
        DoMove(delay);
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

    public void DoMove(float delay = 0)
    {
        if (tween != null && tween.IsPlaying())
        {
            tween.Kill();
        }
        tween = transform.DOMove(position, 1f).SetEase(Ease.InOutQuint).SetDelay(delay);
    }

    #region OnPointerEnter
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable)
        {
            CmdOnPointerEnter();
        }

        if (isOpened)
        {
            info.SetActive(true);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdOnPointerEnter()
    {
        RpcOnPointerEnter();
    }

    [ClientRpc]
    void RpcOnPointerEnter()
    {

    }
    #endregion

    #region OnPointerExit
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelectable)
        {
            CmdOnPointerExit();
        }

        if (info.activeSelf)
        {
            info.SetActive(false);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdOnPointerExit()
    {
        RpcOnPointerExit();
    }

    [ClientRpc]
    void RpcOnPointerExit()
    {

    }
    #endregion
}