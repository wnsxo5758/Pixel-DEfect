using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject weaponPrefab;
    
    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPosition;
    private WeaponBase cachedWeaponData;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    void Start()
    {
        startPosition = transform.position;

        // 무기 프리팹이 있으면 데이터 캐싱
        if (weaponPrefab != null)
        {
            WeaponBase weapon = weaponPrefab.GetComponent<WeaponBase>();
            if (weapon != null)
            {
                cachedWeaponData = weapon;
            }
        }
        
        // 무기 시각적 효과
        if (spriteRenderer != null && weaponPrefab != null)
        {
            SpriteRenderer weaponSprite = weaponPrefab.GetComponent<SpriteRenderer>();
            if (weaponSprite != null)
            {
                spriteRenderer.sprite = weaponSprite.sprite;
            }
        }
    }

    void Update()
    {
        transform.position = startPosition + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0);
    }
    
    // 무기 데이터 가져오기
    public WeaponBase GetWeaponData()
    {
        if (cachedWeaponData != null)
        {
            return cachedWeaponData;
        }
        
        // 캐싱된 데이터가 없으면 새로운 인스턴스 생성
        GameObject tempWeapon = Instantiate(weaponPrefab);
        WeaponBase weaponData = tempWeapon.GetComponent<WeaponBase>();

        // 생성한 게임 오브젝트 즉시 제거 (데이터만 필요)
        tempWeapon.SetActive(false);
        Destroy(tempWeapon);

        return weaponData;
    }
}
