using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchorTest : MonoBehaviour
{
    /*
     * anchoredPosition ��ü�� 
     * UI ������Ʈ����
     * Local Position�� �ǹ̰� �ִ�.
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
