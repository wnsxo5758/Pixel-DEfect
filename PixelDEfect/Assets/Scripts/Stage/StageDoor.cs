using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDoor : InteractableObject
{
    public GameObject[] targets;               // 비활성화 여부를 확인할 대상들
    public GameObject infoUIPrefab;            // UI 프리팹 (우측 상단에 띄울 것)
    public Transform uiParent;                 // UI를 넣을 부모 (예: Canvas)

    private Animator animator;
    private BoxCollider2D boxCollider;
    private bool doorOpened = false;
    private GameObject uiInstance;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // 모든 대상이 꺼졌을 때 문 열기
        if (!doorOpened && CheckAllTargetsInactive())
        {
            isActive = true;
            Active();
            doorOpened = true;
        }

        // isActive 상태에 따라 BoxCollider2D 활성/비활성
        if (boxCollider != null)
        {
            boxCollider.enabled = !isActive;
        }
    }

    private bool CheckAllTargetsInactive()
    {
        foreach (GameObject obj in targets)
        {
            if (obj != null && obj.activeSelf)
                return false;
        }
        return true;
    }

    public override void Trigger()
    {
        isActive = !isActive;
        Active();
    }

    private void Active()
    {
        animator.SetBool("isActive", isActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive && other.CompareTag("Player"))
        {
            // UI가 이미 떠 있다면 중복 생성 방지
            if (uiInstance == null && infoUIPrefab != null)
            {
                uiInstance = Instantiate(infoUIPrefab, uiParent);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (uiInstance != null && other.CompareTag("Player"))
        {
            Destroy(uiInstance);
        }
    }
}
