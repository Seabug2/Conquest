using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource main;

    private void Awake()
    {
        main = GetComponent<AudioSource>();
    }

    private void Start()
    {
        main.volume = 0;
        main.DOFade(1, 4f);
    }
}
