using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : InteractableObject
{
    [Header("레이저 기본설정")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LaserBeam laserPrefab;
    [SerializeField] private Transform laserPos;

    [Header("움직이는 레이저 관련")]
    [SerializeField] private bool canMove;
    [SerializeField] private Transform movePos;
    [SerializeField] private float moveDuration;
    [SerializeField] private float waitDuration;

    private Vector3 originalPos;
    private Coroutine moveRoutine;
    private LaserBeam currentLaser;

    public override void Trigger()
    {
        isActive = !isActive;

        if (isActive)
        {
            ActivateLaser();

            if (canMove && moveRoutine == null)
                moveRoutine = StartCoroutine(MoveRoutine());
        }
        else
        {
            DeactivateLaser();

            if (canMove && moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
        }
    }

    private void Awake()
    {
        originalPos = transform.position;

        // 미리 생성 & 비활성화
        currentLaser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        currentLaser.SetSource(laserPos);
        currentLaser.gameObject.SetActive(false);

        Trigger(); // 초기 전원 상태 설정
    }

    private void Update()
    {
        if (isActive)
        {
            UpdateLaser();
        }
    }

    private void ActivateLaser()
    {
        if (currentLaser != null && !currentLaser.gameObject.activeSelf)
        {
            currentLaser.gameObject.SetActive(true);
        }
    }

    private void DeactivateLaser()
    {
        if (currentLaser != null && currentLaser.gameObject.activeSelf)
        {
            currentLaser.gameObject.SetActive(false);
        }
    }

    private void UpdateLaser()
    {
        if (currentLaser == null || !currentLaser.gameObject.activeSelf) return;

        RaycastHit2D hit = Physics2D.Raycast(laserPos.position, Vector2.down, Mathf.Infinity, groundLayer);
        float laserLength = hit.collider != null ? hit.distance : 100f;

        currentLaser.SetLength(laserLength);
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            yield return MoveTo(movePos.position);
            yield return new WaitForSeconds(waitDuration);
            yield return MoveTo(originalPos);
            yield return new WaitForSeconds(waitDuration);
        }
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
}