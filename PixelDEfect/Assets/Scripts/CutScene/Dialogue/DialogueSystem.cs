using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshPro text;                // 대사를 출력할 텍스트 컴포넌트
    public GameObject TextBox;              // 말풍선 배경 오브젝트

    public float typingSpeed = 0.1f;        // 한 글자 출력 속도
    public float waitAfterLine = 3f;        // 한 대사 출력 후 대기 시간

    private Queue<string> sentences;        // 대사 큐
    private Transform followTarget;         // 따라갈 대상 (보통 캐릭터 머리 위)
    public TextMeshPro sizeCalculatorText;  // 말풍선 크기 계산


    public void StartDialogue(string[] lines, Transform target)
    {
        followTarget = target;
        sentences = new Queue<string>(lines);
        StartCoroutine(DialogueFlow());
    }

    IEnumerator DialogueFlow()
    {
        while (sentences.Count > 0)
        {
            string sentence = sentences.Dequeue();
            yield return StartCoroutine(TypeSentence(sentence));
            yield return new WaitForSeconds(waitAfterLine);
        }

        Destroy(gameObject);    // 모두 출력 후 말풍선 제거
    }

    IEnumerator TypeSentence(string sentence)
    {
        // 미리 박스 크기 계산
        sizeCalculatorText.text = sentence;
        yield return null;      // 다음 프레임까지 기다려야 계산됨

        float width = Mathf.Min(sizeCalculatorText.preferredWidth + 0.3f, 3f);
        float height = sizeCalculatorText.preferredHeight + 0.3f;

        TextBox.transform.localScale = new Vector2(width, height);

        // 실제 대사 출력은 한 글자씩
        text.text = "";
        foreach (char c in sentence)
        {
            text.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void ResizeBox()
    {
        float width = Mathf.Min(text.preferredWidth + 0.3f, 3f);
        TextBox.transform.localScale = new Vector2(width, text.preferredHeight + 0.3f);
    }

    void LateUpdate()
    {
        if (followTarget != null)
        {
            // 위치 고정 (머리 위에 표시)
            transform.position = followTarget.position + new Vector3(0, 1.5f, 0);

            // 좌우 반전 보정: 말풍선은 항상 정방향 유지
            Vector3 targetScale = followTarget.lossyScale;
            float scaleX = targetScale.x >= 0 ? 1f : -1f;

            transform.localScale = new Vector3(scaleX, 1f, 1f);
        }
    }

    public void ShowSingleLine(string sentence, Transform target, float duration)
    {
        followTarget = target;
        StartCoroutine(TypeAndAutoDestroy(sentence, duration));
    }

    void SetBoxSize(string sentence)
    {
        sizeCalculatorText.text = sentence;
        sizeCalculatorText.ForceMeshUpdate(); // 텍스트 길이 정확히 반영
        float width = Mathf.Min(sizeCalculatorText.preferredWidth + 0.3f, 3f);
        float height = sizeCalculatorText.preferredHeight + 0.3f;

        TextBox.transform.localScale = new Vector2(width, height);
    }

    IEnumerator TypeAndAutoDestroy(string sentence, float duration)
    {
        SetBoxSize(sentence);

        text.text = "";
        foreach (char c in sentence)
        {
            text.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
