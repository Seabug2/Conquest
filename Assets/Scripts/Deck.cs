using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    /*
     ���� ����
    
     */
    public List<Villain> pieces;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 1.2f);
    }

    void DrawPhase()
    {
        //ī�带 ��ο� �ϴ� ����
    }
}
