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

    private AudioSource audio;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    private void PlaySound(AudioClip _clip)
    {
        audio.Stop();
        audio.clip = _clip;
        audio.Play();
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
        if (audio.clip == _clip && audio.isPlaying) return;

        audio.loop = true;
        audio.clip = _clip;
        audio.Play();
    }

    public void StopLoopSound()
    {
        if(audio.loop)
        {
            audio.loop = false;
            audio.Stop();
        }
    }
}
