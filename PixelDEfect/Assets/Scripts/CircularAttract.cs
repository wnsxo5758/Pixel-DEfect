using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularAttract : MonoBehaviour
{
    public ParticleSystem ps;
    public float pullSpeed = 5f;

    private ParticleSystem.Particle[] particles;

    void LateUpdate()
    {
        if (ps == null) return;

        if (particles == null || particles.Length < ps.main.maxParticles)
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }

        int count = ps.GetParticles(particles);
        Vector3 center = ps.transform.position;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (center - particles[i].position).normalized;
            particles[i].velocity = dir * pullSpeed;
        }

        ps.SetParticles(particles, count);
    }
}
