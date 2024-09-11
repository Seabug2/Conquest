using UnityEngine;

[CreateAssetMenu]
public class CardInfo : ScriptableObject
{
    public uint id;
    public string cardName;
    public string flavorText;
    public string description;
    public string imagePath;
    public Socket[] skckets = new Socket[4];
}
