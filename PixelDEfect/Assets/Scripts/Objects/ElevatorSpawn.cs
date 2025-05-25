using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorSpawn : ButtonBase
{
    [Header("Elevator Settings")]
    [SerializeField] private ElevatorController elevator;
    [SerializeField] private bool isUpperFloorButton; // true = 위층 버튼
    [SerializeField] private bool isInsideButton;     // true = 내부 버튼

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] spawnPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnDuration = 10f;

    private bool hasSpawnStarted = false;

    public bool IsActive => isActive; // 외부 접근용

    public override void ButtonTrigger()
    {
        Debug.Log("엘리베이터 버튼이 눌림");

        if (elevator == null || elevator.IsMoving)
            return;

        if (isInsideButton)
        {
            StartCoroutine(ButtonActive());
        }
        else
        {
            bool isAtThisFloor = isUpperFloorButton == elevator.IsAtUpperFloor;
            if (isAtThisFloor) return;

            StartCoroutine(ButtonActive());
        }
    }

    protected override IEnumerator ButtonActive()
    {
        isActiving = true;
        isActive = true;
        UpdateSprite();
        audioSoruce.Play();

        elevator.RequestMove();

        // Spawn system 시작 (단 한 번만 실행)
        if (!hasSpawnStarted)
        {
            hasSpawnStarted = true;
            StartCoroutine(SpawnCoroutine());
        }

        yield return new WaitForSeconds(1.0f);
        isActiving = false;
    }

    private IEnumerator SpawnCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            SpawnRandomObject();
            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }
    }

    private void SpawnRandomObject()
    {
        if (spawnPrefabs.Length == 0 || spawnPoint == null) return;

        int index = Random.Range(0, spawnPrefabs.Length);
        Instantiate(spawnPrefabs[index], spawnPoint.position, Quaternion.identity);
    }

    public void SetInteractable(bool active)
    {
        isActive = active;
        UpdateSprite();
        GetComponent<Collider2D>().enabled = active;
    }
}
