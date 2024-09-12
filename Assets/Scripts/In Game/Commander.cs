using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public float delay = 0;
    public Action command;
    public float interval = 0;

    public Command(float delay, Action action, float interval)
    {
        this.delay = Mathf.Max(0, delay); // 음수 방지
        command = action;
        this.interval = Mathf.Max(0, interval);
    }
}

public class Commander : MonoBehaviour
{
    private List<Command> commands = new List<Command>();
    private Coroutine coroutine = null;
    private bool autoClear = true;
    private bool autoKill = true;
    private int loopCount = 1;

    #region Add
    //액션 추가
    public Commander Add(Action action)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(0, action, 0));
        return this;
    }

    /// <summary>
    /// 대기 시간 추가
    /// </summary>
    public Commander Add(float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(0, null, interval));
        return this;
    }

    /// <summary>
    /// 실행 지연 시간, 액션 추가
    /// </summary>
    public Commander Add(float delay, Action action)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(delay, action, 0));
        return this;
    }

    /// <summary>
    /// 실행 지연 시간, 액션, 다음 액션 호출 지연
    /// </summary>
    public Commander Add(float delay, Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(delay, action, interval));
        return this;
    }

    /// <summary>
    /// 액션, 다음 액션 호출 지연
    /// </summary>
    public Commander Add(Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("커맨드 실행 중");
            return this;
        }

        commands.Add(new Command(0, action, interval));
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

    public Commander AutoClear(bool autoClear)
    {
        this.autoClear = autoClear;
        return this;
    }

    public Commander AutoKill(bool autoKill)
    {
        this.autoKill = autoKill;
        return this;
    }

    public Commander SetClear()
    {
        commands.Clear();
        return this;
    }

    public bool IsPlaying => coroutine != null;

    public int GetCount => commands.Count;

    /// <summary>
    /// 저장한 커맨드를 모두 실행합니다.
    /// </summary>
    public void Play(bool autoKill = true)
    {
        if (commands.Count == 0)
        {
            Debug.Log("저장된 커맨드가 없습니다.");
            return;
        }

        if (IsPlaying)
        {
            Debug.Log("커맨더가 실행 중입니다.");
            return;
        }

        this.autoKill = autoKill;
        coroutine = StartCoroutine(Playing_co());
    }

    /// <summary>
    /// 실행을 중단 합니다.
    /// </summary>
    public void Stop()
    {
        if (IsPlaying)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    private IEnumerator Playing_co()
    {
        int l = loopCount;

        while (l != 0)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                yield return new WaitForSeconds(commands[i].delay);
                commands[i].command?.Invoke();
                OnComplete?.Invoke();
                yield return new WaitForSeconds(commands[i].interval);
            }

            OnCompleteAll?.Invoke();

            l--;
        }

        if (autoKill)
        {
            Destroy(this);
        }
        else
        {
            coroutine = null;
            if (autoClear)
                commands.Clear();
        }
    }

    public event Action OnComplete;
    public event Action OnCompleteAll;
}