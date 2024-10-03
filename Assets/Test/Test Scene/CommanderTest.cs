using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommanderTest : MonoBehaviour
{
    void Start()
    {
        float t = 10;
        Commander cmd = new();
        cmd
            .WaitWhile(() => true)
            .OnUpdate(() =>
            {
                t -= Time.deltaTime;
                Debug.Log(Time.time);
            })
            .CancelTrigger(() => Input.GetKeyDown(KeyCode.Space) || t <= 0)
            .OnCanceled(()=>Debug.LogError("Áß´Ü!"))
            .Play();
    }
}
