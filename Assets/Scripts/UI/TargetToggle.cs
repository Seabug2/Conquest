using UnityEngine;

public class TargetToggle : MonoBehaviour
{
    public GameObject target;

    public void Toggle()
    {
        target.SetActive(!target.activeSelf);
    }
}
