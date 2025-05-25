using UnityEngine;

[System.Serializable]
public class WeaponPullContext
{
    [Header("상황 정보")]
    public bool isGrounded;             // 플레이어와 적이 모두 땅에 있는지
    public bool isEnemyAirborne;        // 적이 공중에 있는지
    public bool isPlayerAirborne;       // 플레이어가 공중에 있는지
    
    [Header("무기 정보")]
    public ThrownWeapon targetWeapon;   // 대상 무기
    public Vector3 weaponPosition;      // 무기 위치
    public Vector2 contactNormal;       // 접촉 표면 법선벡터

    [Header("넉백 설정")] 
    public Vector2 knockBackForce;    // 넉백 힘

    [Header("애니메이션 제어")] 
    public bool requiresMovementLock = true;    // 애니메이션 중 이동 금지

    public bool isTeleportPull = false;         // 텔레포트 후 뽑기인지 여부

    public WeaponPullContext(ThrownWeapon weapon, Vector3 playerPos, bool playerGrounded, bool enemyGrounded = true)
    {
        targetWeapon = weapon;
        weaponPosition = weapon.transform.position;
        contactNormal = weapon.GetContactNormal();
        
        isPlayerAirborne = !playerGrounded;
        isEnemyAirborne = !enemyGrounded;
        
        isGrounded = playerGrounded && enemyGrounded;
        isTeleportPull = false;

        if (!isGrounded)
        {
            CalculateKnockBackDirection(playerPos);
        }
    }

    public WeaponPullContext()
    {
        
    }
    
    public static WeaponPullContext CreateTeleportPull(ThrownWeapon weapon, Vector3 playerPos, bool playerGrounded)
    {
        var context = new WeaponPullContext();
        context.targetWeapon = weapon;
        context.weaponPosition = weapon.transform.position;
        context.contactNormal = weapon.GetContactNormal();
        
        // 텔레포트 후 뽑기는 항상 공중 뽑기 취급
        context.isGrounded = false;
        context.isPlayerAirborne = !playerGrounded;
        context.isEnemyAirborne = false;
        context.isTeleportPull = true;
        
        context.CalculateKnockBackDirection(playerPos);

        return context;
    }
    
    private void CalculateKnockBackDirection(Vector3 playerPos)
    {
        // 플레이어에서 무기로의 방향 벡터
        Vector2 toWeapon = (weaponPosition - playerPos).normalized;
        
        // 반대 방향을 ㅗ넉백
        float knockBackX = -toWeapon.x * knockBackForce.x;
        knockBackForce = new Vector2(knockBackX, Mathf.Abs(knockBackForce.y));
    }
}
