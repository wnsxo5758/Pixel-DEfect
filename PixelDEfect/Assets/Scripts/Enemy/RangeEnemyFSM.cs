using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : EnemyFSM
{
    [Header("���Ÿ� ���� ����")]
    [SerializeField]
    private int damage;
    [SerializeField]
    private float coolTime; // ���� ��Ÿ��
    [SerializeField]
    private float currentCoolTime; // ���� ��Ÿ��
    [SerializeField]
    private GameObject bulletPrefab; // �Ѿ� ������
    [SerializeField]
    private Transform firePos; // ������ġ
    [SerializeField]
    private float bulletSpeed;

    protected override IEnumerator Attack()
    {
        Debug.Log("�÷��̾ ���� ����!");
        movement.MoveTo(0);
        // animator.isAttack = true;
        // animator.UpdateAnimation(0);
        while (true)
        {
            if(currentCoolTime <= 0)// ��Ÿ���� 0���� ������
            {
                Shooting(); // �Ѿ� �߻�
                currentCoolTime = coolTime; // ��Ÿ�� �ʱ�ȭ
            }
            else
            {
                currentCoolTime -= Time.deltaTime; // ��Ÿ�� 
            }

            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }

    }

    private void Shooting()
    {
        //�ƹ��͵� ������ return
        if (bulletPrefab == null || firePos == null) return;

        //
        GameObject bullet = Instantiate(bulletPrefab, firePos.position, Quaternion.identity);

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            Vector2 direction = (target.position - firePos.position).normalized; // �÷��̾� ���� ���
            rigid.velocity = direction * bulletSpeed; // �Ѿ� �̵�
        }
    }
}
