using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    public void StartGame() // 게임 시작 버튼 누를 경우
    {
        Debug.Log("게임을 시작합니다");
        StartCoroutine(LoadSceneAsync("Stage1"));
    }
    public void LoadGame() // 이어 하기를 누를 경우
    {
        Debug.Log("게임을 이어합니다");
    }

    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다");
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 UI 표시
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // 1프레임 대기 (UI 갱신 보장)
        yield return null;

        // 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 로딩이 끝날 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 필요 시 로딩바 갱신 (optional)
            yield return null;
        }
    }

}
