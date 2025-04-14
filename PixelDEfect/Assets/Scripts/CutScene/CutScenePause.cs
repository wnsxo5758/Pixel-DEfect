using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutScenePause : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public TextEffect promptBlinker;

    private Coroutine freezeCoroutine;
    private bool isPaused = false;

    void Update()
    {
        if (isPaused && Input.GetKeyDown(KeyCode.F))
        {
            ResumeTimeline();
        }
    }

    public void PauseTimelineExactly()
    {
        isPaused = true;
        timelineDirector.Pause();
        timelineDirector.time = timelineDirector.time;
        timelineDirector.Evaluate();

        freezeCoroutine = StartCoroutine(FreezeTimeline());

        if (promptBlinker != null)
            promptBlinker.StartBlink();
    }

    public void ResumeTimeline()
    {
        isPaused = false;

        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);

        timelineDirector.Play();

        if (promptBlinker != null)
            promptBlinker.StopBlink();
    }

    private IEnumerator FreezeTimeline()
    {
        while (isPaused)
        {
            timelineDirector.time = timelineDirector.time;
            timelineDirector.Evaluate();
            yield return null;
        }
    }
}
