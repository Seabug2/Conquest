public interface IAbility
{
    //카드를 필드에 냈을 때 발동하는 효과
    //연결을 완성했을 때 발동하는 효과
    //완성되어 필드에서 제거할 때 발동하는 효과

    public void SetValue(string[] data);
    public void Activate();
    public void OnCompleteLinked();
    public void OnExit();
}

public class NoneAbility : IAbility
{
    public void Activate() { }

    public void OnCompleteLinked()
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void SetValue(string[] data) { }
}