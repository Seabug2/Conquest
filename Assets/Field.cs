using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Field : MonoBehaviour
{
    public Tile[] tiles = new Tile[12];
    public int seatNum;
#if UNITY_EDITOR
    [SerializeField] float offset = 0f;
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.forward * offset, $"{seatNum}");
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 5));
    }
#endif
}
