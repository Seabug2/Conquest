using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraShakeController : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Awake()
    {
        if (TryGetComponent(out CinemachineVirtualCamera vCam))
            perlinNoise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    Tween tween;

    public void DoShake(float duration, float power)
    {
        if (tween != null && tween.IsActive()) tween.Kill();
        perlinNoise.m_AmplitudeGain = power;

        tween = DOTween.To(() => perlinNoise.m_AmplitudeGain, x => perlinNoise.m_AmplitudeGain = x, 0f, duration);
    }
}
