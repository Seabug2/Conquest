using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAbility : IAbility
{
    int amount;

    public void Activate()
    {
        throw new System.NotImplementedException();
    }

    public void OnCompleteLinked()
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void SetValue(string[] data)
    {
        int.TryParse(data[0], out amount);
    }
}
