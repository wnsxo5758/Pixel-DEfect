using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Press : MonoBehaviour
{
    [SerializeField]
    private float delayTime;
    private Collider2D hitTrigger;
    private ImpactMemoryPool impactMemoryPool;
    private AudioSource audioSource;

    private Animator animator;
    private bool isPressingActive;
    private bool isHit;

    private void Awake()
    {
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        hitTrigger= GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(PressRoutine());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(isPressingActive)
            {
                PlayerHp playerHp = other.GetComponent<PlayerHp>();
                if (playerHp != null)
                {
                    playerHp.Die();
                }
            }

        }
        else if(!isHit)
        {
            Debug.Log("바닥에 충돌됨");
            isHit = true; 

            Vector2 hitPoint = other.ClosestPoint(hitTrigger.transform.position);
            Quaternion rot = Quaternion.identity;

            impactMemoryPool.OnSpawnImpact(ImpactType.Normal, hitPoint, rot);

            if(audioSource != null)
            {
                audioSource.Play();
            }

            StartCoroutine(ResetHit());
        }
    }


    private IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.5f); // 너무 짧으면 연속 충돌 발생, 필요에 따라 조절
        isHit = false;
    }



    private IEnumerator PressRoutine()
    {
        while (true)
        {
            if (animator != null)
            {
                // 1. bool을 true로 설정
                animator.SetBool("isPressing", true);
                isPressingActive = true;
                // 2. 애니메이션 상태가 "Press"로 전환될 때까지 대기
                yield return new WaitUntil(() =>
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.IsName("Press");
                });


                // 3. 애니메이션 길이만큼 대기
                AnimatorStateInfo pressState = animator.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForSeconds(pressState.length);

                isPressingActive = false;
                // 4. 애니메이션 끝나고 false로 끔
                animator.SetBool("isPressing", false);
            }

            // 5. 딜레이 후 다시 루프
            yield return new WaitForSeconds(delayTime);
        }
    }
}
