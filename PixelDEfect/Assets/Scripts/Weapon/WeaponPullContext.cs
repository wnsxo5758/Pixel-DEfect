using UnityEngine;

[System.Serializable]
public class WeaponPullContext
{
    [Header("상황 정보")] 
    public bool isGrounded;             
    
    [Header("무기 정보")]
    public ThrownWeapon targetWeapon;   // 대상 무기
    public Vector3 weaponPosition;      // 무기 위치
    public Vector3 contactNormal;

    [Header("넉백 설정")] 
    public Vector2 knockBackForce;    // 넉백 힘

    [Header("애니메이션 제어")] 
    public bool isTeleportPull = false;         // 텔레포트 후 뽑기인지 여부

    public WeaponPullContext(ThrownWeapon weapon, Vector3 playerPos, bool playerGrounded)
    {
        targetWeapon = weapon;
        weaponPosition = weapon.transform.position;
        contactNormal = weapon.GetContactNormal();
        
        
        isGrounded = playerGrounded;
        isTeleportPull = false;
        
        // 기본 넉백 힘 설정
        knockBackForce = new Vector2(10f, 20f);

        if (!isGrounded)
        {
            CalculateKnockBackDirection(playerPos);
        }
    }

    public WeaponPullContext()
    {
        knockBackForce = new Vector2(10f, 20f);
    }
    
    public static WeaponPullContext CreateTeleportPull(ThrownWeapon weapon, Vector3 playerPos, bool playerGrounded)
    {
        var context = new WeaponPullContext();
        context.targetWeapon = weapon;
        context.weaponPosition = weapon.transform.position;
        context.contactNormal = weapon.GetContactNormal();
        
        context.isGrounded = playerGrounded;
        context.isTeleportPull = true;
        
        if (!playerGrounded)
        {
            context.CalculateKnockBackDirection(playerPos);
        }

        return context;
    }

    public void SetKnockBackForce(Vector2 force)
    {
        knockBackForce = force;
    }
    
    private void CalculateKnockBackDirection(Vector3 playerPos)
    {
        knockBackForce = new Vector2(10f, 20f);
        
        // 플레이어에서 무기로의 방향 벡터
        Vector2 toWeapon = (weaponPosition - playerPos).normalized;
        
        // 반대 방향으로 넉백
        float knockBackX = toWeapon.x * knockBackForce.x;
        knockBackForce = new Vector2(knockBackX, Mathf.Abs(knockBackForce.y));
    }
}
