using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private int meleeDamage;//근접 공격
    [SerializeField]
    private float meleeRange; // 근접 공격 범위
    [SerializeField]
    private float coolTime; // 근접 공격 쿨타임
    [SerializeField]
    private float attackTime;

    private bool isMelee = false; // 근접 공격 중인가? 

    public int Damage;

    public bool IsMelee => isMelee;

    private void Update()
    {
        coolTime += Time.deltaTime;
    }

    public void MeleeAttack()
    {
        if (coolTime < attackTime) return;

        if (coolTime >= attackTime && !isMelee)
        {
            StartCoroutine(nameof(Melee));
        }
    }

    private IEnumerator Melee()
    {
        coolTime = 0;
        isMelee = true;
        yield return new WaitForSeconds(1f);
        isMelee = false;

    }


    private void MeleeCoolTime()
    {

    }

    public void MagicAttack(int _number)
    {

    }
    private void SkillCoolTime(float _time) // 스킬 쿨타임 스킬마다 쿨 타임이 다르기 때문에 파라미터 사용
    {

    }




}
