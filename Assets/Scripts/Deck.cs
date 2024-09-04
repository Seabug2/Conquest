using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    /*
     덱의 역할
    
     */
    public List<Villain> pieces;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    }

    void DrawPhase()
    {
        //카드를 드로우 하는 차례
    }
}
