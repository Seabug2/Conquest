using System;
using System.Collections.Generic;

public interface IAbility
{
    public void SetValue(string[] data);
    public void Activate();
    public void OnCompleteLinked();
    public void OnExit();
}

public class NoneAbility : IAbility
{
    public void Activate() { }
    public void OnCompleteLinked() { }
    public void OnExit() { }
    public void SetValue(string[] data) { }
}

public class AbilityManager
{
    readonly List<Type> abilities = new List<Type>
    {
        typeof(NoneAbility), // = 0
        typeof(DrawAbility), // = 1 
        typeof(DestroyAbilty) // = 2
    };

    public IAbility Create(int id, string[] args)
    {
        if (id < 0 || id >= abilities.Count) return new NoneAbility();

        IAbility ab = (IAbility)Activator.CreateInstance(abilities[id]);
        ab.SetValue(args);
        return ab;
    }
}