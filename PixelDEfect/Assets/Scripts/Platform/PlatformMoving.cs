using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMoving : MonoBehaviour
{
    [SerializeField]
    private Transform target;//움직이는 발판
    [SerializeField]
    private Transform[] wayPoints; // 이동 지점
    [SerializeField]
    private float waitTime; // 대기시간
    [SerializeField]
    private float timeOffset; // 이동시간 = 거리 * timeOffset

    private int wayPointCount; // 이동 가능한 wayPoint 개수
    private int currentIndex = 0; // 현재 wayPoint 인덱스

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(target.transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
    }
}
