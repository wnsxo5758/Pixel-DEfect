using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timelineDirector;

    public CinemachineVirtualCamera playerVCam;     // 새로 만든 플레이어 따라가는 VCam
    public CinemachineVirtualCamera[] cutsceneVCams;

    private bool hasPlayed = false;

    public GameObject playerObject;                 // 플레이어 오브젝트 할당

    private Vector2 originalVelocity;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;
    RuntimeAnimatorController originalAnimator;

    public void DisablePlayerControl()
    {
        playerRb = playerObject.GetComponent<Rigidbody2D>();
        playerAnimator = playerObject.GetComponent<Animator>();

        if (playerRb != null)
        {
            originalVelocity = playerRb.velocity;
            playerRb.velocity = Vector2.zero;
            playerRb.simulated = false;
        }

        if (playerAnimator != null)
        {
            originalAnimator = playerAnimator.runtimeAnimatorController;
            playerAnimator.runtimeAnimatorController = null; // 애니메이션 완전 정지
        }

        playerObject.GetComponent<Collider2D>().enabled = false;
    }

    public void EnablePlayerControl()
    {
        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.velocity = originalVelocity;
        }

        if (playerAnimator != null && originalAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = originalAnimator; // 애니메이터 복구
        }

        playerObject.GetComponent<Collider2D>().enabled = true;
    }

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
            DisablePlayerControl();

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
        EnablePlayerControl();
    }

    public void ForcePlay()
    {
        if (hasPlayed) return;

        hasPlayed = true;
        DisablePlayerControl();

        foreach (var cam in cutsceneVCams)
            cam.Priority = 20;

        playerVCam.Priority = 10;

        timelineDirector.Play();
    }
}
