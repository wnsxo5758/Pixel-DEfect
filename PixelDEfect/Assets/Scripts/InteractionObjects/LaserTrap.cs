using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : InteractableObject
{

    [Header("레이저 설정")]
    [SerializeField]
    private LayerMask groundLayer; // 레이저가 닿을 레이어, 이것에 닿기 전까지는 계속 늘어난다
    [SerializeField]
    private LaserBeam laserPrefab; // 레이저 프리팹
    [SerializeField]
    private float maxLaserLength; // 최대 레이저 길이
    [SerializeField]
    private Transform laserPos; // 레이저가 시작되는 위치

    private LaserBeam currentLaser;
    public override void Trigger()
    {
        ActivateLaser();
    }

    private void Awake()
    {
        ActivateLaser();
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
        isActive = !isActive;

        if (isActive == true) // 활성화한 경우
        {
            if (currentLaser == null)
            {
                currentLaser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
                currentLaser.SetSource(laserPos);
            }
        }
        else if (isActive == false) // 비활성화한 경우
        {
            Destroy(currentLaser);
            if(currentLaser != null)
            {
                Destroy(currentLaser.gameObject);
                currentLaser = null;
            }

        }
    }

    private void UpdateLaser()
    {
        if (currentLaser == null) return;

        RaycastHit2D hit = Physics2D.Raycast(laserPos.position, Vector2.down, maxLaserLength, groundLayer);
        float laserLength = hit.collider != null ? hit.distance : maxLaserLength;

        currentLaser.SetLength(laserLength);
    }
}
