public interface IDamageable
{
    void DecreaseHP(int amount); // 데미지 입기
    void IncreaseHP(int amount); // 힐 또는 회복

    void Death(); // 사망
}
