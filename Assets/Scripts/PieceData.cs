using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PieceData : ScriptableObject
{
    public int id; //카드 아이디
    public string villainName; //빌런 이름
    public string flavorText; //카드 배경 설명
    public string description; //효과 설명
    public Sprite villainImage; //이미지 파일

    // 0   1
    //
    // 3   2
    public Attribute[] sockets;
}
