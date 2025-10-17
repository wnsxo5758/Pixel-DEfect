using UnityEngine;

/// <summary>
/// 제네릭 싱글톤 패턴 베이스 클래스
/// DontDestroyOnLoad 자동 적용 및 중복 방지
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            // 애플리케이션 종료 중에는 인스턴스 생성 안 함
            if (applicationIsQuitting)
            {
                return null;
            }

            if (instance == null)
            {
                // 씬에서 먼저 찾기
                instance = FindAnyObjectByType<T>();

                // 없으면 자동 생성
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        // 이미 인스턴스가 존재하고, 그게 나 자신이 아니면 파괴
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name} 중복 감지! 기존 인스턴스 유지, 새 인스턴스 파괴.");
            Destroy(gameObject);
            return;
        }

        // 첫 번째 인스턴스 설정
        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        // 내가 현재 인스턴스면 null로 설정
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}
