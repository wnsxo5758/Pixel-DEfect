using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("플레이어 관련 사운드")]
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
    [SerializeField]
    private AudioClip deathClip;
    [SerializeField]
    private AudioClip hitClip;

    [Header("공격 관련 사운드")]
    [SerializeField]
    private AudioClip teleportClip;
    [SerializeField]
    private AudioClip attackClip;
    [SerializeField]
    private AudioClip throwClip;

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


    public void DeadthSound() // 사망 효과음
    {
        PlaySound(deathClip);
    }
    public void HitSound()// 피격 효과음
    {
        PlaySound(hitClip);
    }
    public void StartTeleport() // 텔레포트 효과음
    {
        PlaySound(teleportClip);
    }

    public void JumpSound() // 점프 효과음
    {
        PlaySound(jumpClip);
    }
    public void LandSound() // 착지 효과음
    {
        PlaySound(landClip);
    }
    public void MeleeAttackSound()
    {
        PlaySound(attackClip);
    }
    public void ThrowAttackSound()
    {
        PlaySound(throwClip);
    }


    public void LoopMoveSound()
    {
        LoopPlaySound(walkClip);
    }
    public void LoopRunSound()
    {
        LoopPlaySound(runClip);
    }
    public void StopMoveSound()
    {
        if(audioSoruce.clip == walkClip)
        {
            audioSoruce.loop = false;
            audioSoruce.Stop();
        }
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
