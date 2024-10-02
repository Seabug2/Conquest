using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchorTest : MonoBehaviour
{
    public
    RectTransform[] parents;
    int num = 0; 
    public
    RectTransform text;

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
