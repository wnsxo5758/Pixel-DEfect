using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    [Header("Buttons")]
    public List<GameObject> connectedObjects; // 버튼 오브젝트들

    [Header("CutSceneTrigger")]
    public GameObject cutsceneTriggerObject; // 컷씬 재생 로직이 들어있는 오브젝트

    private bool hasPlayedCutscene = false;

    void Update()
    {
        if (hasPlayedCutscene) return;

        bool allActivated = true;

        foreach (var obj in connectedObjects)
        {
            if (obj == null)
            {
                allActivated = false;
                break;
            }

            // 모든 컴포넌트 중 isActiving 변수를 가진 컴포넌트를 리플렉션으로 탐색
            bool foundActive = false;
            var components = obj.GetComponents<MonoBehaviour>();

            foreach (var comp in components)
            {
                var type = comp.GetType();
                var field = type.GetField("isActiving", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (field != null)
                {
                    object value = field.GetValue(comp);
                    if (value is bool isActive && isActive)
                    {
                        foundActive = true;
                        break;
                    }
                }
            }

            if (!foundActive)
            {
                allActivated = false;
                break;
            }
        }

        if (allActivated)
        {
            hasPlayedCutscene = true;
            PlayCutscene();
        }
    }

    private void PlayCutscene()
    {
        if (cutsceneTriggerObject == null)
        {
            Debug.LogWarning("컷씬 트리거 오브젝트가 할당되지 않았습니다.");
            return;
        }

        // 컷씬 트리거 오브젝트에서 ForcePlay() 호출
        var cutsceneTrigger = cutsceneTriggerObject.GetComponent<CutsceneTrigger>();
        if (cutsceneTrigger != null)
        {
            cutsceneTrigger.ForcePlay();
        }
        else
        {
            Debug.LogWarning("CutsceneTrigger 컴포넌트를 찾을 수 없습니다.");
        }
    }
}
