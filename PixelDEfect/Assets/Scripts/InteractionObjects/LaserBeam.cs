using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("레이저 빔설정")]
    [SerializeField]
    private int damage = 1;// 데미지
    [SerializeField]
    private Transform beamTransform; // 레이저의 실제 Transforem 
    [SerializeField]
    private LayerMask damageAbleLayer; // 데미지를 줄 레이어

    private Transform source;
    public void SetSource(Transform laserTrap)
    {
        source = laserTrap;
        transform.position = source.position;
    }

    public void SetLength(float length)
    {
        if (beamTransform == null) return;

        beamTransform.localScale = new Vector3 (1,length, 1);
        beamTransform.position = source.position + Vector3.down * (length / 2);
    }

}
