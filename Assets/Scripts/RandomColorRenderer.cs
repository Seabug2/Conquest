using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColorRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Random.ColorHSV());
    }
}
