using UnityEngine;
using UnityEngine.Events;

public class AutoStart : MonoBehaviour
{
    public UnityEvent OnStartEvent;

    void Start()
    {
        OnStartEvent?.Invoke();
    }
}
