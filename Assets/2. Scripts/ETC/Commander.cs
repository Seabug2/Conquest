using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public Action command { get; private set; } //실행 후...
    public float Interval { get; private set; } //~초 동안 대기
    public Func<bool> Until { get; private set; } //~까지 실행 대기
    public Func<bool> While { get; private set; } //~동안 실행 대기

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
/// Action과 익명 메서드를 리스트에 저장합니다.
/// <para>Add()를 통해 명령과 명령 실행 후 대기할 시간을 설정할 수 있습니다.</para>
/// <para>Play()하여 실행할 수 있습니다.</para>
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
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(action));
        return this;
    }

    public Commander WaitSeconds(float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(interval));
        return this;
    }

    public Commander WaitUntil(Func<bool> until)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(until));
        return this;
    }

    public Commander WaitWhile(Func<bool> @while)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(@while, 0));
        return this;
    }



    public Commander Add(Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(action, interval));
        return this;
    }
    public Commander Add_Until(Action action, Func<bool> until)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(action, until));
        return this;
    }
    public Commander Add_While(Action action, Func<bool> @while)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(action, @while, 0));
        return this;
    }
    #endregion



    /// <summary>
    /// 0 보다 작은 수를 넣으면 무한히 반복합니다.
    /// </summary>
    public Commander SetLoop(int loopCount = -1)
    {
        if (loopCount == 0) loopCount = 1;
        this.loopCount = loopCount;
        return this;
    }

    /// <summary>
    /// 작업을 마치면 입력받은 커맨드를 전부 삭제합니다.
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
    /// 저장한 커맨드를 모두 실행합니다.
    /// </summary>
    public Commander Play(bool autoClear = true)
    {
        this.autoClear = autoClear;

        if (commands.Count == 0)
        {
            Debug.Log("저장된 커맨드가 없습니다.");
            return this;
        }

        if (IsPlaying)
        {
            Debug.Log("커맨더가 실행 중입니다.");
            return this;
        }

        cancel?.Cancel();
        cancel = new CancellationTokenSource();

        Task(cancel.Token).Forget();
        Update(cancel.Token).Forget();
        return this;
    }

    /// <summary>
    /// 실행을 중단 합니다.
    /// </summary>
    public void Cancel()
    {
        if (IsPlaying && cancel != null)
        {
            cancel.Cancel();  // 작업 취소
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

                //~초 동안 대기
                if (cmd.Interval > 0)
                    await UniTask.Delay(TimeSpan.FromSeconds(cmd.Interval), cancellationToken: token).SuppressCancellationThrow();

                //~까지 대기
                if (cmd.Until != null)
                    await UniTask.WaitUntil(cmd.Until, cancellationToken: token).SuppressCancellationThrow();

                //~까지 대기
                if (cmd.While != null)
                    await UniTask.WaitWhile(cmd.While, cancellationToken: token).SuppressCancellationThrow();

                // 취소되었는지 확인
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