
using UnityEngine;

public struct DeathData
{
    public DeathCause cause;        // 사망 원인
    public float direction;

    public DeathData(DeathCause cause, float direction = 0)
    {
        this.cause = cause;
        this.direction = direction;
    }
}
