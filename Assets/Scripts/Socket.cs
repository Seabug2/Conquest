using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    isNull = 0,
    power,
    tech,
    rich,
    charisma
}

public class Socket : MonoBehaviour
{
    public Socket type;
    public bool isFilled;
    public Token lickedCard = null;
}
