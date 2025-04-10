using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timelineDirector;

    public CinemachineVirtualCamera playerVCam;   // 새로 만든 플레이어 따라가는 VCam
    public CinemachineVirtualCamera[] cutsceneVCams;

    private bool hasPlayed = false;

    private void OnEnable()
    {
        timelineDirector.stopped += OnCutsceneEnd;
    }

    private void OnDisable()
    {
        timelineDirector.stopped -= OnCutsceneEnd;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("Player"))
        {
            hasPlayed = true;

            // 컷씬 카메라 우선순위 높임
            foreach (var cam in cutsceneVCams)
                cam.Priority = 20;

            playerVCam.Priority = 10;

            timelineDirector.Play();
        }
    }

    private void OnCutsceneEnd(PlayableDirector director)
    {
        // 컷씬 끝나고 플레이어 카메라 복귀
        playerVCam.Priority = 20;

        foreach (var cam in cutsceneVCams)
            cam.Priority = 10;
    }

    public void ForcePlay()
    {
        if (hasPlayed) return;

        hasPlayed = true;

        foreach (var cam in cutsceneVCams)
            cam.Priority = 20;

        playerVCam.Priority = 10;

        timelineDirector.Play();
    }
}
