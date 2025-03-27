using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : BulletBase
{
    private Vector2 direction; // 총알이 날아갈 방향

    public void SetUp(Vector2 targetPosition) // 플레이어 위치를 받아 방향 설정
    {
        direction = (targetPosition - (Vector2)transform.position).normalized; // 방향 벡터 설정
        RotateToTarget(direction); // 방향에 맞춰 회전
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime); // 설정된 방향으로 이동
    }

    private void RotateToTarget(Vector2 dir) // 총알이 날아가는 방향으로 회전
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    protected override IEnumerator DestoryBullet()
    {
        direction = Vector2.zero;
        Collider2D collider = GetComponent<Collider2D>();
        if(collider != null) collider.enabled = false;

        if(animator != null)
        {
            animator.SetTrigger("isHit");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }
        Destroy(gameObject);
    }
}

