using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ManaAnimator : MonoBehaviour
{
    private Animator animator;
    private EnemyAnimator baseAnimator;

    // Hash된 파라미터 이름들
    private readonly int castSkill = Animator.StringToHash("Skill");
    private readonly int manaDeath = Animator.StringToHash("ManaDeath");
    private readonly int jump = Animator.StringToHash("Jump");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        baseAnimator = GetComponent<EnemyAnimator>();

        if (animator == null)
            Debug.LogError("Animator 컴포넌트가 없습니다!");

        if (baseAnimator == null)
            Debug.LogWarning("EnemyAnimator 컴포넌트가 없습니다!");
    }
    public void TriggerSkillAnim()
    {
        animator?.SetTrigger(castSkill);
    }
    public void OnSkillEvent()
    {
        HammerBT hammer = transform.GetComponentInParent<HammerBT>();
        hammer.OnSkillEffectTrigger();
    }

    public void OnSkillFinished()
    {
        HammerBT hammer = transform.GetComponentInParent<HammerBT>();
        hammer.OnSkillAnimationFinished();
    }

    public void TriggerJump()
    {
        //animator?.SetTrigger(jump);
    }
}
