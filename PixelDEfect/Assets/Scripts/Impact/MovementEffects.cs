using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementEffects : MonoBehaviour
{
    private MovementRigidbody2D movement;

    [Header("플레이어 이동시")]
    [SerializeField]
    private ParticleSystem footStepEffect;
    private ParticleSystem.EmissionModule footEmisson;

    [Header("플레이어 착지시")]
    [SerializeField]
    private ParticleSystem landingEffect;
    private bool wasOnGround;

    private void Awake()
    {
        movement = GetComponentInParent<MovementRigidbody2D>();
        footEmisson = footStepEffect.emission;

    }

    private void Update()
    {
        //플레이어가 바닥에 있고 이동속도가 0이 아닌 경우
        if(movement.IsGrounded && movement.Velocity.x != 0)
        {
            footEmisson.rateOverTime = 30;
        }
        else
        {
            footEmisson.rateOverTime = 0;
        }

        if(!wasOnGround && movement.IsGrounded && movement.Velocity.y <= 0)
        {
            landingEffect.Stop();
            landingEffect.Play();
        }
        wasOnGround = movement.IsGrounded;
    }
}
