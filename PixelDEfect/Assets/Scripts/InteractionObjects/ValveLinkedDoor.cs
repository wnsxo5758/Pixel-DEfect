using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValveLinkedDoor : MonoBehaviour
{
    [SerializeField]
    private ValveButton linkedValve;
    [SerializeField]
    private float maxHeight = 3f; // 문이 완전히 열렸을 때의 높이
    private bool isLocked;
    private Vector3 initialPosition;
    private float prevRatio;

    private BoxCollider2D col;
    private float originalHeight;
    private Vector2 originalOffset;


    private void Start()
    {
        col = GetComponent<BoxCollider2D>();

        if (col != null)
        {
            originalHeight = col.size.y;
            originalOffset = col.offset;
        }
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (linkedValve == null) return;

        float ratio = linkedValve.GetPressRatio();

        // 완전히 열렸을 때
        if (ratio >= 1f)
        {
            isLocked = true;
        }

        // 수치가 줄어들면 고정 해제
        if (ratio < prevRatio)
        {
            isLocked = false;
        }

        // 문이 잠겨있으면 위치 유지
        if (!isLocked)
        {
            transform.position = initialPosition + Vector3.up * (maxHeight * ratio);

            if (col != null)
            {
                float newHeight = Mathf.Lerp(0f, originalHeight, 1f - ratio); // 위로 열릴수록 작아짐
                float offsetAdjust = (originalHeight - newHeight) * 0.5f;
                col.size = new Vector2(col.size.x, newHeight);
                col.offset = originalOffset - new Vector2(0, offsetAdjust);
            }
        }

        prevRatio = ratio;
    }
}
