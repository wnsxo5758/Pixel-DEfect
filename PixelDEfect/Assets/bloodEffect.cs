using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bloodEffect : MonoBehaviour
{

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // 애니메이션을 항상 처음부터 재생
        if (animator != null)
        {
            animator.Rebind(); // 상태 리셋
            animator.Update(0f); // 즉시 반영
        }
    }

    public void DisableSelf()
    {
        gameObject.SetActive(false);
    }
}
