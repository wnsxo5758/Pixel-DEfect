using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : BulletBase
{
    private Vector2 direction; // �Ѿ��� ���ư� ����

    public void SetUp(Vector2 targetPosition) // �÷��̾� ��ġ�� �޾� ���� ����
    {
        direction = (targetPosition - (Vector2)transform.position).normalized; // ���� ���� ����
        RotateToTarget(direction); // ���⿡ ���� ȸ��
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime); // ������ �������� �̵�
    }

    private void RotateToTarget(Vector2 dir) // �Ѿ��� ���ư��� �������� ȸ��
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    protected override IEnumerator DestroyBullet()
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

