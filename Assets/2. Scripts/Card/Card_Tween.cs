using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Mirror;
using DG.Tweening;

public partial class Card : NetworkBehaviour
{
    public Vector3 TargetPosition { get; private set; }

    public void SetTargetPosition(float x, float y, float z)
        => TargetPosition.Set(x, y, z);

    public void SetTargetPosition(Vector3 targetPosition)
        => TargetPosition = targetPosition;


    public Quaternion TargetRotation { get; private set; }

    public void SetTargetQuaternion(float x, float y, float z, float w)
        => TargetRotation.Set(x, y, z, w);

    public void SetTargetQuaternion(Quaternion targetRotation)
        => TargetRotation = targetRotation;


    const float duration = 0.5f;

    [Command(requiresAuthority = false)]
    public void CmdDoMove()
    {
        RpcDoMove();
    }

    [ClientRpc]
    void RpcDoMove()
    {
        DoMove();
    }

    [Client]
    public void DoMove(float delay = 0, Ease setEase = Ease.Unset)
    {
        transform.DOKill();
        transform.DOMove(TargetPosition, duration).SetEase(setEase).SetDelay(delay);
        transform.DORotateQuaternion(TargetRotation, duration).SetEase(setEase).SetDelay(delay);
    }

    [Client]
    public void DoMove(Action onCompleteEvent, float delay = 0, Ease setEase = Ease.Unset)
    {
        transform.DOKill();
        transform.DORotateQuaternion(TargetRotation, duration).SetEase(setEase).SetDelay(delay);
        transform.DOMove(TargetPosition, duration)
            .SetEase(setEase).SetDelay(delay)
            .OnComplete(() => onCompleteEvent?.Invoke());
    }



    [Command(requiresAuthority = false)]
    public void CmdPick(float _height)
    {
        RpcPick(_height);
    }

    [ClientRpc]
    void RpcPick(float _height)
    {
        Pick(_height);
    }

    [Client]
    public void Pick(float _height)
    {
        transform.DOKill();
        transform.rotation = Quaternion.identity;
        Vector3 pos = TargetPosition;
        pos.y = _height;
        transform.position = pos;
    }



    [Command(requiresAuthority = false)]
    public void CmdReturnToDeck(float delay)
    {
        RpcReturnToDeck(delay);
    }

    [ClientRpc]
    public void RpcReturnToDeck(float delay)
    {
        ReturnToDeck(delay);
    }

    [Client]
    public void ReturnToDeck(float delay = 0)
    {
        ownerOrder = -1;
        iCardState = GameManager.instance.noneState;
        IsOpened = false;
        
        TargetPosition = GameManager.deck.transform.position;
        TargetRotation = GameManager.deck.transform.rotation;

        transform.DOKill();
        transform.DORotateQuaternion(TargetRotation, duration).SetDelay(delay);
        transform.DOMove(TargetPosition, duration).SetDelay(delay);
    }
}
