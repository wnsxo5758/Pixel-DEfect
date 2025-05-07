using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeAffectedEntity : MonoBehaviour, ITimeAffected
{
    protected bool isTimeFrozen = false;
    protected List<DamageInfo> pendingDamages = new List<DamageInfo>();
    
    // 인터페이스 메서드 구현
    public virtual void OnTimeStop() { isTimeFrozen = true; }
    public virtual void OnTimeResume() { isTimeFrozen = false; }

    // 시간 정지 중 데미지 처리
    public virtual void ReceiveDamageInFrozenTime(int damage, Vector2 direction)
    {
        pendingDamages.Add(new DamageInfo(damage, direction));
    }
    
    // 시간 정지 범위 내인지 확인
    public virtual bool IsInTimeFreezeRange(Vector3 originPosition, float range)
    {
        return Vector3.Distance(originPosition, transform.position) <= range;
    }
    
    // 데미지 정보 구조체
    protected struct DamageInfo
    {
        public int damage;
        public Vector2 direction;

        public DamageInfo(int damage, Vector2 direction)
        {
            this.damage = damage;
            this.direction = direction;
        }
    }
}
