public interface IAbility
{
    //ī�带 �ʵ忡 ���� �� �ߵ��ϴ� ȿ��
    //������ �ϼ����� �� �ߵ��ϴ� ȿ��
    //�ϼ��Ǿ� �ʵ忡�� ������ �� �ߵ��ϴ� ȿ��

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