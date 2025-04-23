using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LaserDirection { Down =0, Up , Right, Left}
public class LaserTrap : InteractableObject
{
    [Header("레이저 기본설정")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LaserBeam laserPrefab;
    [SerializeField] private Transform laserPos;
    [SerializeField]
    private LaserDirection laserDirection;

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
    private void ApplyRotationByDirection()
    {
        float zRotation = laserDirection switch
        {
            LaserDirection.Up => 180f,
            LaserDirection.Down => 0f,
            LaserDirection.Left => -90f,
            LaserDirection.Right => 90f,
            _ => 0f
        };

        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    private void Awake()
    {
        laserSetUp();
        Trigger(); // 초기 전원 상태 설정
    }
    private void laserSetUp()
    {
        originalPos = transform.position;
        ApplyRotationByDirection();
        currentLaser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        currentLaser.SetSource(laserPos);
        currentLaser.SetDirection(laserDirection);
        currentLaser.gameObject.SetActive(false);
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
            currentLaser.Deactivate(); // ✅ 이펙트 정리
            currentLaser.gameObject.SetActive(false);
        }
    }

    private void UpdateLaser()
    {
        if (currentLaser == null || !currentLaser.gameObject.activeSelf) return;

        currentLaser.SetLength(100f); // 적당한 최대 길이만 넘겨주면, 내부에서 Raycast 방향 처리함
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