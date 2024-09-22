using UnityEngine;
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


    [Command(requiresAuthority = false)]
    public void CmdPick()
    {
        RpcPick();
    }

    [ClientRpc]
    void RpcPick()
    {
        Pick();
    }

    [Client]
    public void Pick()
    {
        transform.DOKill();
        transform.rotation  = Quaternion.identity;
        transform.position = TargetPosition + transform.up * 0.35f;
    }
}
