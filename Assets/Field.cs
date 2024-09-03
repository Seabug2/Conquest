using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Field : MonoBehaviour
{
    public Tile[] tiles = new Tile[12];
    public int seatNum;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(5, .1f, 5));
    }
}
