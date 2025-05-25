using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BossCutSceneTrigger : MonoBehaviour
{
    public PlayableDirector cutscene; // ½ÇÇàÇÒ ÄÆ¾À
    private int deathCount = 0;
    private bool playerInZone = false;
    private PlayerHp playerHp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            playerHp = other.GetComponent<PlayerHp>();

            if (playerHp != null)
                playerHp.OnPlayerDeath += OnPlayerDeath;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;

            if (playerHp != null)
                playerHp.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    private void OnPlayerDeath()
    {
        if (!playerInZone) return;

        deathCount++;
        Debug.Log("¿µ¿ª ³» »ç¸Á È½¼ö: " + deathCount);

        if (deathCount >= 3)
        {
            TriggerCutscene();
        }
    }

    private void TriggerCutscene()
    {
        if (cutscene != null)
        {
            cutscene.Play();
            Debug.Log("ÄÆ¾À ½ÇÇàµÊ");
        }
    }
}
