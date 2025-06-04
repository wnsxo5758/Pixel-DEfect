using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;            // 이동할 씬 이름
    [SerializeField] private string targetTag = "Player";   // 충돌할 태그
    [SerializeField] private float transitionDelay = 0.5f;  // 전환 지연 시간
    [SerializeField] private bool savePlayerState = true;   // 플레이어 상태 저장 여부

    private bool isTransitioning = false;

    [SerializeField] private GameObject loadingPanel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag) && !isTransitioning)
        {
            StartCoroutine(TransitionToScene(other.gameObject));
        }
    }

    private IEnumerator TransitionToScene(GameObject player)
    {
        isTransitioning = true;
        
        // 플레이어 상태 저장
        if (savePlayerState && PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.SavePlayerState(player);
        }
        
        // 타임 매니저 이벤트 정리
        if (TimeManager.Instance != null && TimeManager.Instance.IsTimeFrozen())
        {
            TimeManager.Instance.ResumeTime();
        }
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        yield return new WaitForSeconds(transitionDelay);
        
        // 씬 로드
        SceneManager.LoadScene(sceneToLoad);
    }
}
