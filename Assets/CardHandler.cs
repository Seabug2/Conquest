using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public class CardHandler : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 카드가 앞면이라면 마우스를 위에 올렸을 때 카드 정보 UI를 활성화한다.
    /// </summary>
    [SyncVar, SerializeField]
    bool isOpened;

    SpriteRenderer sprite;

    [HideInInspector]
    public bool isMine;
    [Header("카드의 상세정보 UI"), Tooltip("앞 면으로 공개된 카드는 마우스를 올려두면 카드 정보 UI가 나타납니다.")]
    public GameObject ui;

    /// <summary>
    /// (나의 패에서)조작 가능한 상태인지를 나타내는 bool 값
    /// </summary>
    public bool isSelectable;
    readonly Vector3 onPointerScale = new Vector3(1.2f, 1.2f, 1.2f);

    public Vector3 targetPosition;
    public Vector3 targetRotation;

    [Command]
    void CmdSetTargetPosition(float x, float y, float z)
    {
        RpcSetTargetPosition(x, y, z);
    }
    [ClientRpc]
    void RpcSetTargetPosition(float x, float y, float z)
    {
        targetPosition.Set(x, y, z);
        RpcDoMove();
    }

    //PositionKeeper positionkeeper;
    Camera cam;
    Tween tween;

    void Start()
    {
        cam = Camera.main;
        isSelectable = false;
        isOpened = false;
    }

    /*
     * 
     */
    #region Drag Event
    public void OnBeginDrag(PointerEventData eventData)
    {
        //만약 tween이 실행 중이라면...
        if (isSelectable)
        {
            if (tween.IsPlaying())
            {
                tween.Kill();
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

        if (isSelectable)
        {
            DoMove();
        }
    }
    #endregion

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

    void DoMove()
    {
        if (tween.IsPlaying())
        {
            tween.Kill();
        }
        tween = transform.DOMove(targetPosition, 1f).SetEase(Ease.InOutQuint);
    }

    #region OnPointerEnter
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectable)
        {
            CmdScaleUp();
        }

        if (isOpened || isMine)
        {
            ui.SetActive(true);
        }
    }

    [Command]
    void CmdScaleUp()
    {
        RpcScaleUp();
    }

    [ClientRpc]
    void RpcScaleUp()
    {
        transform.localScale = onPointerScale;
    }
    #endregion

    #region OnPointerExit
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelectable)
        {
            CmdResetScale();
        }

        if (isOpened || isMine)
        {
            ui.SetActive(false);
        }
    }

    [Command]
    void CmdResetScale()
    {
        RpcResetScale();
    }

    [ClientRpc]
    void RpcResetScale()
    {
        transform.localScale = Vector3.one;
    }
    #endregion
}