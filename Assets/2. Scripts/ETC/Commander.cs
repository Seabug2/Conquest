using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public Action command { get; private set; } //���� ��...
    public float Interval { get; private set; } //~�� ���� ���
    public Func<bool> Until { get; private set; } //~���� ���� ���
    public Func<bool> While { get; private set; } //~���� ���� ���

    public Command(Action command)
    {
        Interval = 0;
        Until = null;
        While = null;
        this.command = command;
    }
    public Command(Func<bool> until)
    {
        Interval = 0;
        While = null;
        this.Until = until;
        command = null;
    }
    public Command(Func<bool> @while, int interval = 0)
    {
        this.Interval = interval;
        this.While = @while;
        Until = null;
        command = null;
    }
    public Command(float interval)
    {
        this.Interval = interval;
        command = null;
        While = null;
        Until = null;
    }


    public Command(Action command, Func<bool> until)
    {
        Interval = 0;
        this.Until = until;
        While = null;
        this.command = command;
    }
    public Command(Action command, Func<bool> @while, int interval = 0)
    {
        Until = null;
        this.Interval = interval;
        this.While = @while;
        this.command = command;
    }
    public Command(Action command, float interval)
    {
        this.Interval = interval;
        this.command = command;
        While = null;
        Until = null;
    }
}

/// <summary>
/// Action�� �͸� �޼��带 ����Ʈ�� �����մϴ�.
/// <para>Add()�� ���� ��ɰ� ��� ���� �� ����� �ð��� ������ �� �ֽ��ϴ�.</para>
/// <para>Play()�Ͽ� ������ �� �ֽ��ϴ�.</para>
/// </summary>
public class Commander
{
    private List<Command> commands = new List<Command>();
    private int loopCount = 1;
    bool autoClear = true;
    public bool IsPlaying => cancel != null && !cancel.IsCancellationRequested;

    public Commander()
    {
        loopCount = 1;
        autoClear = true;
    }

    public Commander(bool autoClear)
    {
        this.autoClear = autoClear;
    }
    public Commander(int loopCount)
    {
        SetLoop(loopCount);
    }
    public Commander(bool autoClear, int loopCount)
    {
        this.autoClear = autoClear;
        SetLoop(loopCount);
    }



    #region Add
    public Commander Add(Action action)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(action));
        return this;
    }

    public Commander WaitSeconds(float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(interval));
        return this;
    }

    public Commander WaitUntil(Func<bool> until)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(until));
        return this;
    }

    public Commander WaitWhile(Func<bool> @while)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(@while, 0));
        return this;
    }



    public Commander Add(Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(action, interval));
        return this;
    }
    public Commander Add_Until(Action action, Func<bool> until)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(action, until));
        return this;
    }
    public Commander Add_While(Action action, Func<bool> @while)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(action, @while, 0));
        return this;
    }
    #endregion



    /// <summary>
    /// 0 ���� ���� ���� ������ ������ �ݺ��մϴ�.
    /// </summary>
    public Commander SetLoop(int loopCount = -1)
    {
        if (loopCount == 0) loopCount = 1;
        this.loopCount = loopCount;
        return this;
    }

    /// <summary>
    /// �۾��� ��ġ�� �Է¹��� Ŀ�ǵ带 ���� �����մϴ�.
    /// </summary>
    public Commander SetAutoClear(bool autoClear = true)
    {
        this.autoClear = autoClear;
        return this;
    }

    public Commander Clear()
    {
        commands.Clear();
        return this;
    }

    public int CommandCount => commands.Count;
    int taskCount = 0;

    public float TaskProgress
    {
        get
        {
            if (CommandCount == 0)
            {
                return 0;
            }
            else
            {
                return Mathf.Clamp((float)taskCount / CommandCount, 0, 1);
            }
        }
    }

    /// <summary>
    /// ������ Ŀ�ǵ带 ��� �����մϴ�.
    /// </summary>
    public Commander Play(bool autoClear = true)
    {
        this.autoClear = autoClear;

        if (commands.Count == 0)
        {
            Debug.Log("����� Ŀ�ǵ尡 �����ϴ�.");
            return this;
        }

        if (IsPlaying)
        {
            Debug.Log("Ŀ�Ǵ��� ���� ���Դϴ�.");
            return this;
        }

        cancel?.Cancel();
        cancel = new CancellationTokenSource();

        Task(cancel.Token).Forget();
        Update(cancel.Token).Forget();
        return this;
    }

    /// <summary>
    /// ������ �ߴ� �մϴ�.
    /// </summary>
    public void Cancel()
    {
        if (IsPlaying && cancel != null)
        {
            cancel.Cancel();  // �۾� ���
        }
    }


    private async UniTask Task(CancellationToken token)
    {
        int l = loopCount;
        taskCount = 0;
        while (l != 0 && !token.IsCancellationRequested)
        {
            foreach (Command cmd in commands)
            {
                cmd.command?.Invoke();

                //~�� ���� ���
                if (cmd.Interval > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(cmd.Interval), cancellationToken: token).SuppressCancellationThrow();

                //~���� ���
                if (cmd.Until != null)
                    await UniTask.WaitUntil(cmd.Until, cancellationToken: token).SuppressCancellationThrow();

                //~���� ���
                if (cmd.While != null)
                    await UniTask.WaitWhile(cmd.While, cancellationToken: token).SuppressCancellationThrow();

                // ��ҵǾ����� Ȯ��
                if (token.IsCancellationRequested)
                    break;

                onComplete?.Invoke();
                taskCount = Mathf.Clamp(taskCount + 1, 0, CommandCount);
            }

            onCompleteAll?.Invoke();

            l--;
        }

        if (autoClear)
        {
            onComplete = null;
            onCompleteAll = null;
            onUpdate = null;
            ClearCancelTrigger();
        }

        if (!token.IsCancellationRequested)
        {
            cancel.Cancel();
        }
    }

    private CancellationTokenSource cancel;


    Action onComplete;
    public Commander OnComplete(Action onComplete)
    {
        this.onComplete = onComplete;
        return this;
    }


    Action onCompleteAll;
    public Commander OnCompleteAll(Action onCompleteAll)
    {
        this.onCompleteAll = onCompleteAll;
        return this;
    }


    Action onUpdate;
    public Commander OnUpdate(Action onUpdate)
    {
        this.onUpdate = onUpdate;
        return this;
    }

    async UniTask Update(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            onUpdate?.Invoke();

            if (cancelTrigger())
            {
                cancel.Cancel();
                onCanceled?.Invoke();
            }

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
        }
    }

    Func<bool> cancelTrigger = () => { return false; };
    public Commander CancelTrigger(Func<bool> cancelTrigger)
    {
        this.cancelTrigger = cancelTrigger;
        return this;
    }

    Action onCanceled;
    public Commander OnCanceled(Action onCanceled)
    {
        this.onCanceled = onCanceled;
        return this;
    }

    public Commander ClearCancelTrigger()
    {
        cancelTrigger = () => { return false; };
        return this;
    }
}