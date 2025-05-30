using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx { Dead, Hit, Run, Walk, Melee, Jump,Land } // 
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGM")]
    [SerializeField]
    private AudioClip bgmClip;      // 배경음
    [SerializeField]
    private AudioSource bgmPlayer;  // 배경음 담당
    [SerializeField, Range(0, 100)]
    private float bgmVolume = 100;

    [Header("#SFX")]
    [SerializeField]
    private AudioClip[] sfxClips;
    [SerializeField]
    private float sfxVolume;
    [SerializeField]
    private int channels;
    int channelIndex;


    AudioSource[] sfxPlayers;

    private void Awake()
    {
        instance = this;
        Init();
        StartBGM();
    }

    private void StartBGM()
    {
        bgmPlayer.Play();
    }
    private void Init()
    {
        //배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer"); // 배경음 담당 오브젝트 생성
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();  // 배경음 담당 오브젝트에 사운드 소스 추가
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume / 100f;
        bgmPlayer.clip = bgmClip;

        //효과음 플레이어 초기화

        GameObject sfxObject = new GameObject("SfxPlayer"); // 효과음 담당 오브젝트 생성
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;
            if (sfxPlayers[loopIndex].isPlaying)
            {
                continue;
            }
            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    public void ChangeBGM(AudioClip newClip, float volume = 100f)
    {
        if (bgmPlayer.clip == newClip)
            return;

        bgmPlayer.Stop();
        bgmPlayer.clip = newClip;
        bgmPlayer.volume = volume / 100f;
        bgmPlayer.Play();
    }

    public void PlaySfxLoop(Sfx sfx)
    {

    }
}
