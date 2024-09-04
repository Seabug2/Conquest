using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Shake : MonoBehaviour
{
    public float shakeTime = 0;

    public float amplitude = 0;
    public float frequancy = 0;
    
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Start()
    {
        if(TryGetComponent(out CinemachineVirtualCamera vCam))
        perlinNoise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
        }
        else
        {
            enabled = false;
        }
    }
}
