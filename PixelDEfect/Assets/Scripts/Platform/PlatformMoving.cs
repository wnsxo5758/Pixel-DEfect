using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMoving : MonoBehaviour
{
    [SerializeField]
    private Transform target;//�����̴� ����
    [SerializeField]
    private Transform[] wayPoints; // �̵� ����
    [SerializeField]
    private float waitTime; // ���ð�
    [SerializeField]
    private float timeOffset; // �̵��ð� = �Ÿ� * timeOffset

    private int wayPointCount; // �̵� ������ wayPoint ����
    private int currentIndex = 0; // ���� wayPoint �ε���

    public bool IsMoving { get; private set; } = true;

    private void Awake()
    {
        target.position = wayPoints[currentIndex].position;
        wayPointCount = wayPoints.Length;
        currentIndex++;
        StartCoroutine(nameof(Process));
    }


    private IEnumerator Process()
    {
        while (true)
        {
            yield return StartCoroutine(MoveAToB(target.position, wayPoints[currentIndex].position));

            yield return new WaitForSeconds(waitTime);

            if (currentIndex < wayPointCount - 1) currentIndex++;
            else currentIndex = 0;
        }
    }

    private IEnumerator MoveAToB(Vector3 start, Vector3 end)
    {
        float percent = 0;
        float moveTime = Vector3.Distance(start, end) * timeOffset;

        while (percent < 1)
        {
            percent += Time.deltaTime / moveTime;
            target.position = Vector3.Lerp(start, end, percent);
            yield return null;
        }
    }

    // 시간 정지 관련 메서드
    public void PauseMovement()
    {
        IsMoving = false;
        
        // 현재 이동 중인 코루틴 중지 등 필요한 처리
    }

    public void ResumeMovement()
    {
        IsMoving = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(target.transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
    }
}
