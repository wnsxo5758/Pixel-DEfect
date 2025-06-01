using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV2Button : ButtonBase
{
    [Header("페이드아웃UI")]
    [SerializeField]
    private FadeOutUI fadeOutUI;
    public override void ButtonTrigger()
    {
        StartCoroutine(ButtonActive());
    }

    protected override IEnumerator ButtonActive()
    {
        isActiving = true;
        isActive = true;
        audioSoruce.Play();

        fadeOutUI.FadeStart();
        yield return null;

    }
}
