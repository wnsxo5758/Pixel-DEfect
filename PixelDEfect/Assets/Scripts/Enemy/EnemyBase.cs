using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField]
    private int maxHp;
    private int currentHp;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

    [SerializeField]
    private float checkWallDistance; // 벽 확인 거리
    [SerializeField]
    private LayerMask wallLayer; // 벽 레이어
    [SerializeField]
    private bool isFacingRight = true; // true인 경우 우측을 보는 중

    [Header("효과음")]
    [SerializeField]
    private AudioClip hitClip; // 피격 효과음
    [SerializeField]
    private AudioClip deadClip; // 사망시 효과음
    [SerializeField]
    private AudioClip attackClip; // 
    [Header("사망시 보상")]
    [SerializeField]
    private GameObject coins; // 사망시 드랍하는 코인
    [SerializeField]
    private int amount; // 드랍하는 코인의 최댓값


    private AudioSource audio;
    private Vector2 dir;

    private bool isChange;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }


    public bool IsFacingRight => isFacingRight;

    public void DecreaseHp(int _damage)
    {
        if (currentHp > 0)
        {
            currentHp -= _damage;
            PlaySound(hitClip);
            if (currentHp <= 0)
            {
                Debug.Log($"{gameObject.name}가 사망");
                OnDead();
            }
        }
    }

    public void IncreaseHp(int _amount)
    {
        if (currentHp < maxHp)
        {
            currentHp += _amount;
            if (currentHp > maxHp)
            {
                currentHp = maxHp;
            }
        }
    }
    public void ChangeFacing()
    {
        if (isChange == true)
        {
            return;
        }

        StartCoroutine(nameof(Changing));

    }

    private IEnumerator Changing()
    {
        isChange = true;
        isFacingRight = !isFacingRight;
        Debug.Log("돌았다!!!");
        yield return new WaitForSeconds(1f);
        isChange = false;
    }
    public bool CheckWall()
    {
        dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, checkWallDistance, wallLayer);
        if (hit.collider != null)
        {
            if (!isChange)
            {
                Debug.Log($"벽 감지 : + {hit.collider.name}");
                ChangeFacing();
            }
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, dir * checkWallDistance);
    }
    private void PlaySound(AudioClip clip)
    {
        audio.Stop();
        audio.clip = clip;
        audio.Play();
    }
    public void OnDead()
    {

        PlaySound(deadClip);
    }

}
