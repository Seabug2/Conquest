using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PieceData : ScriptableObject
{
    public int id; //ī�� ���̵�
    public string villainName; //���� �̸�
    public string flavorText; //ī�� ��� ����
    public string description; //ȿ�� ����
    public Sprite villainImage; //�̹��� ����

    // 0   1
    //
    // 3   2
    public Attribute[] sockets;
}
