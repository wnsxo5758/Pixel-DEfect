using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    [Header("카메라 제한")]
    [SerializeField]
    private float cameraLimitMinX;
    [SerializeField]
    private float cameraLimitMaxX;

    [Header("플레이어 제한")]
    [SerializeField]
    private float playerLmitMinX;
    [SerializeField]
    private float playerLmitMaxX;

    [Header("맵 제한")]
    [SerializeField]
    private float mapLimitMinY;

    public float CameraLimitMinX => cameraLimitMinX;
    public float CameraLimitMaxX => cameraLimitMaxX;

    public float PlayerLimitMinX => playerLmitMinX;
    public float PlayerLimitMaxX => playerLmitMaxX;

    public float MapLimitMinY => mapLimitMinY;
}
