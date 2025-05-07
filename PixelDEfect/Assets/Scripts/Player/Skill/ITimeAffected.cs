using UnityEngine;

public interface ITimeAffected
{
    void OnTimeStop();
    void OnTimeResume();
    void ReceiveDamageInFrozenTime(int damage, Vector2 direction);
    bool IsInTimeFreezeRange(Vector3 originPosition, float range);
}
