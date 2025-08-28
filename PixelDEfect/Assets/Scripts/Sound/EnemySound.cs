using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySound : MonoBehaviour
{
    [Header("")]
    [SerializeField]
    private AudioClip hitClip;
    [SerializeField]
    private AudioClip attackClip;
    [SerializeField]
    private AudioClip deadClip;

    AudioSource audioSource;
    EnemyHp enemyHp;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        enemyHp = GetComponentInParent<EnemyHp>();
    }
    private void OnEnable()
    {
        if (enemyHp != null)
        {
            enemyHp.OnDamaged += OnDamaged;
            enemyHp.OnDied += OnDied;
        }
    }



    private void OnDisable()
    {
        if (enemyHp != null)
        {
            enemyHp.OnDamaged -= OnDamaged;
            enemyHp.OnDied -= OnDied;
        }
    }
    private void OnDamaged(int _, bool __) => Play(hitClip);
    private void OnDied() => Play(deadClip);
    public void PlayAttack() => Play(attackClip);


    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

}
