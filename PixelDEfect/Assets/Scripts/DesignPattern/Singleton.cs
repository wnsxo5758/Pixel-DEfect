using UnityEngine;

//싱글톤 
public class Singleton<T> : MonoBehaviour where T : Component
{
    //인스턴스화
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //현재 씬에 해당 타입의 오브젝트를 찾기
                instance = (T)FindAnyObjectByType(typeof(T));
                //없으면 SetUpInstance로 생성
                if (instance == null)
                {
                    SetUpInstance();
                }
            }
            return instance;
        }
    }

    //시작시 싱글턴화
    protected virtual void Awake()
    {
        RemoveDuplicates();
    }
    private static void SetUpInstance()
    {
        //인스턴스가 없는 경우 생성
        instance = (T)FindAnyObjectByType(typeof(T));
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = typeof(T).Name;
            instance = gameObj.AddComponent<T>();
            DontDestroyOnLoad(gameObj);
        }
    }


    //싱글턴이 이미 존재하는 경우 파괴하는 코드
    private void RemoveDuplicates()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
