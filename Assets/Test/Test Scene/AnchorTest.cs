using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchorTest : MonoBehaviour
{
    /*
     * anchoredPosition 자체가 
     * UI 오브젝트에겐
     * Local Position의 의미가 있다.
     */

    public
    RectTransform[] parents;
    public
    RectTransform text;
    int num = 0; 

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            num = (num + 1) % parents.Length;
            Vector3 offset = text.anchoredPosition;
            text.transform.parent = parents[num].transform;
            text.anchoredPosition = offset;
        }
    }
}
