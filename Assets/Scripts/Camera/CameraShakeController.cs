using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeController : MonoBehaviour
{
    public float shakeTime = 0;

    public float amplitude = 0;
    public float frequancy = 100;

    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Awake()
    {
        if (TryGetComponent(out CinemachineVirtualCamera vCam))
            perlinNoise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    float initialTime;
    float size;
    
    private void OnEnable()
    {
        if (shakeTime < 0) shakeTime = 1;
        initialTime = shakeTime;
        perlinNoise.m_AmplitudeGain = amplitude;
        perlinNoise.m_FrequencyGain = frequancy;
    }

    private void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            size = Mathf.InverseLerp(0, initialTime, shakeTime);
            perlinNoise.m_AmplitudeGain = amplitude * size;
        }
        else
        {
            perlinNoise.m_AmplitudeGain = 0;
            enabled = false;
        }
    }
}
