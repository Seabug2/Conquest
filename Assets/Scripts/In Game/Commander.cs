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
        this.delay = Mathf.Max(0, delay); // ���� ����
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
    //�׼� �߰�
    public Commander Add(Action action)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(0, action, 0));
        return this;
    }

    /// <summary>
    /// ��� �ð� �߰�
    /// </summary>
    public Commander Add(float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(0, null, interval));
        return this;
    }

    /// <summary>
    /// ���� ���� �ð�, �׼� �߰�
    /// </summary>
    public Commander Add(float delay, Action action)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(delay, action, 0));
        return this;
    }

    /// <summary>
    /// ���� ���� �ð�, �׼�, ���� �׼� ȣ�� ����
    /// </summary>
    public Commander Add(float delay, Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(delay, action, interval));
        return this;
    }

    /// <summary>
    /// �׼�, ���� �׼� ȣ�� ����
    /// </summary>
    public Commander Add(Action action, float interval)
    {
        if (IsPlaying)
        {
            Debug.Log("Ŀ�ǵ� ���� ��");
            return this;
        }

        commands.Add(new Command(0, action, interval));
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
    /// ������ Ŀ�ǵ带 ��� �����մϴ�.
    /// </summary>
    public void Play(bool autoKill = true)
    {
        if (commands.Count == 0)
        {
            Debug.Log("����� Ŀ�ǵ尡 �����ϴ�.");
            return;
        }

        if (IsPlaying)
        {
            Debug.Log("Ŀ�Ǵ��� ���� ���Դϴ�.");
            return;
        }

        this.autoKill = autoKill;
        coroutine = StartCoroutine(Playing_co());
    }

    /// <summary>
    /// ������ �ߴ� �մϴ�.
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