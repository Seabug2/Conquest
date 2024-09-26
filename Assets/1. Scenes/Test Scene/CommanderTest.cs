using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommanderTest : MonoBehaviour
{
    void Start()
    {

        bool zz = true;
        Commander cmd = new();
        cmd
            .WaitWhile(()=> zz)
            .OnUpdate(() =>
            {
                Debug.Log(Time.time);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    cmd.Cancel();
                }
            })
            .OnCompleteAll(()=> Debug.Log("asd"))
            .Play();
    }
}
