using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    [Header("Object Settings")] 
    [SerializeField] private LayerMask groundLayer;

    private Vector2 collisionSize;
    private Vector2 collisionPoint;
    
    private Collider2D collider;

    public bool IsGrounded { get; private set; } = false;
    
    void Start()
    {
        collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        UpdateCollision();
    }

    public void UpdateCollision()
    {
        Bounds bounds = collider.bounds;
        
        collisionSize = new Vector2((bounds.max.x - bounds.min.x), 0.1f);
        collisionPoint = new Vector2(bounds.center.x, bounds.min.y);
        
        IsGrounded = Physics2D.OverlapBox(collisionPoint, collisionSize, 0, groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        
        Gizmos.DrawWireCube(collisionPoint, collisionSize);
    }
}
