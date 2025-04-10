using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Press : MonoBehaviour
{
    [SerializeField]
    private float delayTime;
    private Collider2D hitTrigger;
    private ImpactMemoryPool impactMemoryPool;
    private AudioSource audio;


    private bool isCoroutineRunning;
    private bool isHit;
    private void Awake()
    {
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        hitTrigger= GetComponent<Collider2D>();
        audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit|| isCoroutineRunning) return;
        if(other.CompareTag("ImpactNormal") || other.CompareTag("ImpactObstacle"))
        {
            Debug.Log("¹Ù´Ú¿¡ Ãæµ¹µÊ");
            isHit = true; 
            Vector2 hitPoint = other.ClosestPoint(hitTrigger.transform.position);
            Quaternion rot = Quaternion.identity;

            ImpactType type = other.CompareTag("ImpactNormal") ? ImpactType.Normal : ImpactType.Obstacle;
            impactMemoryPool.OnSpawnImpact(type, hitPoint, rot);

            if(audio != null)
            {
                audio.Play();
            }

            StartCoroutine(ParticleDelay());
        }

    }


    private IEnumerator ParticleDelay()
    {
        isCoroutineRunning = true;
        yield return new WaitForSeconds(delayTime);
        isHit = false;
        isCoroutineRunning = false;
    }
}
