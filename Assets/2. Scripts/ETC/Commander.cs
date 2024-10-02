using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

#region Command
public class Command
{
    public Action Execute { get; }
    public float Delay { get; }
    public Func<bool> WaitUntilCondition { get; }
    public Func<bool> WaitWhileCondition { get; }

    public Command(Action execute, float delay = 0, Func<bool> waitUntil = null, Func<bool> waitWhile = null)
    {
        Execute = execute;
        Delay = delay;
        WaitUntilCondition = waitUntil;
        WaitWhileCondition = waitWhile;
    }
}
#endregion

public class Commander
{
    private readonly List<Command> commands = new List<Command>();
    private bool autoClear = true;
    private int loopCount = 1;
    private CancellationTokenSource cancellationTokenSource;

    private Action onComplete;
    private Action onCompleteAll;
    private Action onUpdate;
    private Action onCanceled;
    private Func<bool> cancelTrigger = () => false;

    private bool isCanceled = false;

    public bool IsPlaying => cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested;
    public int CommandCount => commands.Count;
    public float TaskProgress => CommandCount == 0 ? 0 : Mathf.Clamp((float)currentTaskIndex / CommandCount, 0, 1);

    private int currentTaskIndex = 0;

    public Commander(bool autoClear = true, int loopCount = 1)
    {
        this.autoClear = autoClear;
        SetLoop(loopCount);
    }

    #region Event Handlers
    public Commander OnComplete(Action callback)
    {
        onComplete = callback;
        return this;
    }

    public Commander OnCompleteAll(Action callback)
    {
        onCompleteAll = callback;
        return this;
    }

    public Commander OnUpdate(Action callback)
    {
        onUpdate = callback;
        return this;
    }

    public Commander OnCanceled(Action callback)
    {
        onCanceled = callback;
        return this;
    }

    public Commander CancelTrigger(Func<bool> trigger)
    {
        cancelTrigger = trigger;
        return this;
    }

    public Commander ClearCancelTrigger()
    {
        cancelTrigger = () => false;
        return this;
    }
    #endregion

    #region Command Management
    public Commander Add(Action action, float delay = 0, Func<bool> waitUntil = null, Func<bool> waitWhile = null)
    {
        if (IsPlaying)
        {
            Debug.Log("Commander is already running.");
            return this;
        }

        commands.Add(new Command(action, delay, waitUntil, waitWhile));
        return this;
    }

    public Commander WaitSeconds(float delay)
    {
        return Add(null, delay);
    }

    public Commander WaitUntil(Func<bool> condition)
    {
        return Add(null, waitUntil: condition);
    }

    public Commander WaitWhile(Func<bool> condition)
    {
        return Add(null, waitWhile: condition);
    }

    public Commander SetLoop(int count = -1)
    {
        loopCount = count == 0 ? 1 : count;
        return this;
    }

    public Commander SetAutoClear(bool autoClear)
    {
        this.autoClear = autoClear;
        return this;
    }

    public Commander Clear()
    {
        commands.Clear();
        return this;
    }
    #endregion

    #region Execution Control
    public Commander Play()
    {
        if (commands.Count == 0)
        {
            Debug.Log("No commands to execute.");
            return this;
        }

        if (IsPlaying)
        {
            Debug.Log("Commander is already running.");
            return this;
        }

        isCanceled = false;
        cancellationTokenSource = new CancellationTokenSource();

        ExecuteCommands(cancellationTokenSource.Token).Forget();
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            MonitorUpdate(cancellationTokenSource.Token).Forget();

        return this;
    }

    public void Cancel()
    {
        if (IsPlaying)
        {
            isCanceled = true;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            onCanceled?.Invoke();
        }
    }

    public Commander Refresh(bool cleanUp = true)
    {
        if (IsPlaying)
        {
            Cancel();
        }
        if (cleanUp)
        {
            Cleanup();
        }
        return this;
    }

    public Commander Cleanup()
    {
        if (IsPlaying)
        {
            Debug.Log("Cannot clean up while running.");
            return this;
        }
        commands.Clear();
        onComplete = null;
        onCompleteAll = null;
        onUpdate = null;
        onCanceled = null;
        cancelTrigger = () => false;
        Debug.Log("Clean up");
        return this;
    }
    #endregion

    #region Execution Methods
    private async UniTask ExecuteCommands(CancellationToken token)
    {
        try
        {
            bool infiniteLoop = loopCount < 0;
            int remainingLoops = loopCount;

            while ((infiniteLoop || remainingLoops > 0) && !token.IsCancellationRequested)
            {
                currentTaskIndex = 0;

                foreach (var command in commands)
                {
                    command.Execute?.Invoke();

                    if (command.Delay > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(command.Delay), cancellationToken: token);

                    if (command.WaitUntilCondition != null)
                        await UniTask.WaitUntil(command.WaitUntilCondition, cancellationToken: token);

                    if (command.WaitWhileCondition != null)
                        await UniTask.WaitWhile(command.WaitWhileCondition, cancellationToken: token);

                    if (token.IsCancellationRequested)
                        return;

                    currentTaskIndex++;
                    onComplete?.Invoke();
                }

                if (!infiniteLoop)
                    remainingLoops--;
            }

        }
        finally
        {
            if (!token.IsCancellationRequested && !isCanceled)
            {
                isCanceled = true;
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                onCompleteAll?.Invoke();
            }

            if (autoClear)
            {
                Cleanup();
            }
        }
    }

    private async UniTask MonitorUpdate(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                onUpdate?.Invoke();

                if (cancelTrigger())
                {
                    Cancel();
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
        }
        finally
        {
            if (!token.IsCancellationRequested && !isCanceled)
            {
                isCanceled = true;
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
    #endregion
}
