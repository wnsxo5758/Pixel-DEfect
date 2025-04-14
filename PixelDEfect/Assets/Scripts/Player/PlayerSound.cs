using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("È¿°úÀ½")]
    [SerializeField]
    private AudioClip walkClip;
    [SerializeField]
    private AudioClip runClip;
    [SerializeField]
    private AudioClip crawlClip;
    [SerializeField]
    private AudioClip climbClip;
    [SerializeField]
    private AudioClip jumpClip;
    [SerializeField]
    private AudioClip landClip;

    private AudioSource audioSoruce;

    private void Awake()
    {
        audioSoruce = GetComponent<AudioSource>();
    }

    private void PlaySound(AudioClip _clip)
    {
        audioSoruce.Stop();
        audioSoruce.clip = _clip;
        audioSoruce.Play();
    }

    public void JumpSound()
    {
        PlaySound(jumpClip);
    }
    public void LandSound()
    {
        PlaySound(landClip);
    }
    public void LoopMoveSound()
    {
        LoopPlaySound(walkClip);
    }
    public void LoopRunSound()
    {
        LoopPlaySound(runClip);
    }

    private void LoopPlaySound(AudioClip _clip)
    {
        if (audioSoruce.clip == _clip && audioSoruce.isPlaying) return;

        audioSoruce.loop = true;
        audioSoruce.clip = _clip;
        audioSoruce.Play();
    }

    public void StopLoopSound()
    {
        if(audioSoruce.loop)
        {
            audioSoruce.loop = false;
            audioSoruce.Stop();
        }
    }
}
