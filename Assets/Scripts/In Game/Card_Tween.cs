using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;
using Mirror;

public partial class Card : NetworkBehaviour
{
    public Vector3 TargetPosition { get; private set; }

    public void SetPosition(float x, float y, float z)
        => TargetPosition.Set(x, y, z);

    public void SetPosition(Vector3 targetPosition)
        => this.TargetPosition = targetPosition;


    public Quaternion TargetRotation { get; private set; }
    
    public void SetQuaternion(float x, float y, float z, float w)
        => TargetRotation.Set(x, y, z, w);

    public void SetQuaternion(Quaternion targetRotation)
        => this.TargetRotation = targetRotation;



    const float duration = 0.5f;

    public void DoMove(float delay = 0, Ease setEase = Ease.InOutQuint)
    {
        transform.DOKill();
        transform.DOMove(TargetPosition, duration).SetEase(setEase).SetDelay(delay);
        transform.DORotateQuaternion(TargetRotation, duration).SetEase(setEase).SetDelay(delay);
    }
}