using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovingTimeAffected : MovingObjectTimeAffected
{
    private PlatformMoving platformMoving;
    private bool wasMoving;

    protected override void Awake()
    {
        base.Awake();
        platformMoving = GetComponent<PlatformMoving>();
    }

    public override void OnTimeStop()
    {
        base.OnTimeStop();
        
        // 플랫폼 이동 상태 저장 및 정지
        if (platformMoving != null)
        {
            // 이동 중이었는지 상태 저장
            wasMoving = platformMoving.IsMoving;
            
            // 이동 중지 메서드 호출
        }
    }
}
