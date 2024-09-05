using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    [SerializeField] float angleOffset = 10f;

    private void Start()
    {
        SetRandomgRotate();
    }

    public void SetRandomgRotate()
    {
        transform.localRotation = Quaternion.Euler(0, Random.Range(-angleOffset, angleOffset), 0);
    }
}
