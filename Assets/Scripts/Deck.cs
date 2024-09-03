using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<Piece> tokens;

    private void Start()
    {
        Piece[] chipArray = FindObjectsOfType<Piece>();
        System.Array.Sort(chipArray, (chip1, chip2) => chip1.ID.CompareTo(chip2.ID));
        tokens = chipArray.ToList();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    }
}
