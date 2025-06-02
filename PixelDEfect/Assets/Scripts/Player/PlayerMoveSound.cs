using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveSound : MonoBehaviour
{


    [Header("πﬂ∞…¿Ω")]
    [SerializeField]
    private AudioClip stepOneClip;
    [SerializeField]
    private AudioClip stepTwoClip;

    AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void FirstStep()
    {
        PlaySound(stepOneClip);
    }
    public void SecondStep() 
    {
        PlaySound(stepTwoClip);
    }

    private void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
