using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    public void StartGame() // ���� ���� ��ư ���� ���
    {
        Debug.Log("������ �����մϴ�");
        StartCoroutine(LoadSceneAsync("Stage1"));
    }
    public void LoadGame() // �̾� �ϱ⸦ ���� ���
    {
        Debug.Log("������ �̾��մϴ�");
    }

    public void ExitGame()
    {
        Debug.Log("������ �����մϴ�");

        #if UNITY_EDITOR
        // Unity 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
        #endif
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // �ε� UI ǥ��
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // 1������ ��� (UI ���� ����)
        yield return null;

        // �� �񵿱� �ε�
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // �ε��� ���� ������ ���
        while (!asyncLoad.isDone)
        {
            // �ʿ� �� �ε��� ���� (optional)
            yield return null;
        }
    }

}
