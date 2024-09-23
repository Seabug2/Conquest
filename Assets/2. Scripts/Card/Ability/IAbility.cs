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
        typeof(NoneAbility),
        typeof(DrawAbility),
        typeof(DestroyAbilty)
    };

    public IAbility Create(int id, string[] args)
    {
        if (id < 0 || id >= abilities.Count) return null;

        IAbility ab = (IAbility)Activator.CreateInstance(abilities[id]);
        ab.SetValue(args);
        return ab;
    }
}