using UnityEngine;

public enum Attribute
{
    isNull = 0,
    power = 1,
    tech = 2,
    wealth = 3,
    charisma = 4
}

public class Socket : MonoBehaviour
{
    public Attribute attribute = Attribute.isNull;
    public bool isFilled = false;
}
