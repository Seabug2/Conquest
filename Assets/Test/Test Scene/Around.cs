using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Around : MonoBehaviour
{
    public Transform[] cards;

    public float radius = 1f;
    public float randomRange = 10f;
    [ContextMenu("Do")]
    public void Setting()
    {
        foreach (Transform c in cards)
        {
            c.position = Vector3.up * 100f;
        }

        int count = cards.Length;
        float intervalAngle = 360f / count;
        float angle = Random.Range(0, 90);

        for (int i = 0; i < count; i++)
        {
            angle += intervalAngle;
            float radian = Mathf.Deg2Rad * angle;
            Vector3 position = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * radius;
            cards[i].position = transform.position + position;
            cards[i].rotation = Quaternion.Euler(0, 0, Random.Range(-randomRange, randomRange));
        }
    }
}
