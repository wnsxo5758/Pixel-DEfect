using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMChanger : MonoBehaviour
{
    [SerializeField] private AudioClip newBgm;
    [SerializeField] private float newVolume = 100f; // 0 ~ 100

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;

            AudioManager.instance.ChangeBGM(newBgm, newVolume);
        }
    }

    public void ChangeBGMBySignal()
    {
        if (triggered) return;

        triggered = true;

        AudioManager.instance.ChangeBGM(newBgm, newVolume);
    }
}
