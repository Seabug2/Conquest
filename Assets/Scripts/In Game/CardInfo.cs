using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardInfo : ScriptableObject
{
    public uint id;
    public string cradName;
    public string flavorText;
    public string description;

    public Socket[] skckets = new Socket[4];
}
