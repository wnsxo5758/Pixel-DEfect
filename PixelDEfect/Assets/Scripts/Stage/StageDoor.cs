using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDoor : InteractableObject
{
    [Header("기본 설정")]
    public GameObject[] targets;               // 확인할 오브젝트들
    public GameObject infoUIPrefab;            // UI 프리팹 (옵션 상황에 뜨는 것)
    public Transform uiParent;                 // UI를 넣을 부모 (예: Canvas)
    
    private bool hasBossTarget = false;  // 보스 처치 확인 여부
    private ManagerRobotBoss targetBoss = null;   // 연결된 보스
    
    private Animator animator;
    private BoxCollider2D boxCollider;
    private bool doorOpened = false;
    private GameObject uiInstance;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        FindBossInTarget();
    }

    private void Start()
    {
        // 보스가 targets에 있으면 보스 이벤트 구독
        if (hasBossTarget)
        {
            BossEvents.OnBossTransformedToRemains += OnBossTransformedToRemains;
        }
        
        // isActive ���¿� ���� BoxCollider2D Ȱ��/��Ȱ��
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (hasBossTarget)
        {
            BossEvents.OnBossTransformedToRemains -= OnBossTransformedToRemains;
        }
    }
    
    private void Update()
    {
        if (doorOpened) return;

        if (!hasBossTarget && CheckAllTargetsInactive() && !doorOpened)
        {
            OpenDoor();
            Debug.Log("StageDoor: 모든 타겟이 비활성화됨, 문을 엽니다.");
        }
    }

    private void OpenDoor()
    {
        doorOpened = true;
        isActive = true;
        Active();
        
        // if (boxCollider != null)
        // {
        //     boxCollider.enabled = !isActive;
        // }
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
            // UI�� �̹� �� �ִٸ� �ߺ� ���� ����
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

    private void FindBossInTarget()
    {
        targetBoss = null;

        if (targets == null || targets.Length == 0) return;

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                ManagerRobotBoss boss = target.GetComponent<ManagerRobotBoss>();
                if (boss != null)
                {
                    targetBoss = boss;
                    Debug.Log($"StageDoor: 보스 발견 - {boss.name}");
                    break; // 첫 번째 보스만 찾으면 종료
                }
            }
        }
        
        hasBossTarget = targetBoss != null;
    }
    
    // 보스가 잔해 상태로 변환되었을 때 호출되는 메서드
    private void OnBossTransformedToRemains(ManagerRobotBoss defeatedBoss)
    {
        if (!hasBossTarget) return;  // ✅ 수정됨
    
        // targets에 있는 보스가 처치되면 문 열기
        if (targetBoss == defeatedBoss)  // ✅ 수정됨
        {
            Debug.Log($"StageDoor: 연동된 보스 {defeatedBoss.name} 처치 확인, 문을 엽니다!");
            OpenDoor();
        }
    }
}
