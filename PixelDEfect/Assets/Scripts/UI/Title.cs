using UnityEngine;

/// <summary>
/// 타이틀 UI 컨트롤러
/// GameManager를 통해 게임 흐름 제어
/// Fade 효과는 SceneSystem에서 자동으로 처리됨
/// </summary>
public class Title : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 버튼 클릭 시
    /// </summary>
    public void StartGame()
    {
        Debug.Log("게임을 시작합니다");

        // GameManager를 통해 게임 시작
        // Fade 효과는 SceneSystem이 자동으로 처리
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("Title: GameManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 이어하기 버튼 클릭 시
    /// </summary>
    public void LoadGame()
    {
        Debug.Log("게임을 이어합니다");

        // TODO: 저장된 데이터가 있다면 해당 스테이지로 이동
        // 현재는 게임 시작과 동일하게 동작
        StartGame();
    }

    /// <summary>
    /// 게임 종료 버튼 클릭 시
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("게임을 종료합니다");

#if UNITY_EDITOR
        // Unity 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
