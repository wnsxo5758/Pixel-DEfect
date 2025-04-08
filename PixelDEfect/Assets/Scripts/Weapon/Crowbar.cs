using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowbar : WeaponBase
{
    [Header("Crowbar")] 
    [SerializeField] private float attackAngle = 60f;
    

    public override void Attack(Vector2 origin, Vector2 direction)
    {
        Vector2 attackDir = direction.normalized;

        Collider2D[] hitEnemies = GetEnemiesInSector(origin, attackDir, 
                                                        attackRange, attackAngle);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyFSM enemyFSM = enemy.GetComponent<EnemyFSM>();
            if (enemyFSM != null)
            {
                enemyFSM.DecreaseHp((int)damage);
                Debug.Log("Hit");
                // 히트 효과음
                
                // 히트 이펙트
                
            }
        }
    }
    
    // 부채꼴 범위 내 적 감지
    private Collider2D[] GetEnemiesInSector(Vector2 origin, Vector2 direction, float radius, float angle)
    {
        // 원형 범위 내 모든 충돌체 감지
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(origin, radius, 
                                        LayerMask.GetMask("Enemy"));
        List<Collider2D> enemiesInSector = new List<Collider2D>();

        // 각 충돌체가 부채꼴 범위 내 있는지 확인
        foreach (Collider2D collider in allColliders)
        {
            Vector2 dirToEnemy = ((Vector2)collider.transform.position - origin).normalized;
            float angleToEnemy = Vector2.Angle(direction, dirToEnemy);

            if (angleToEnemy <= angle / 2)
            {
                enemiesInSector.Add(collider);
            }
        }
        
        return enemiesInSector.ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector2 direction = Vector2.right;

        float startAngle = -attackAngle / 2;
        float endAngle = attackAngle / 2;

        Gizmos.color = Color.magenta;
        Vector2 previousPoint = Vector2.zero;

        for (float angle = startAngle; angle <= endAngle; angle += 5)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector2 point = new Vector2(
                Mathf.Cos(radian) * direction.x - Mathf.Sin(radian) * direction.y,
                Mathf.Sin(radian) * direction.x + Mathf.Cos(radian) * direction.y).normalized * attackRange;

            if (angle > startAngle)
            {
                Gizmos.DrawLine(transform.position + (Vector3)previousPoint, transform.position + (Vector3)point);
            }
            
            previousPoint = point;
        }
        
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)previousPoint);
        Gizmos.DrawLine(transform.position, transform.position + 
                                            new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad), 0) * attackRange);
    }
}
