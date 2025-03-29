using PlayerStates;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private BoxCollider2D topPlatform;

    private void Awake()
    {
        if (topPlatform == null)
        {
            topPlatform = transform.GetComponentInChildren<BoxCollider2D>();
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            float vertical = player.VerticalInput();

            if (Mathf.Abs(vertical) > 0 && !player.IsOnLadder)
            {
                player.ChangeState(new Climb());
                topPlatform.isTrigger = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player.IsOnLadder)
            {
                player.ChangeState(new Idle());
                Debug.Log("Test");
            }
            
            topPlatform.isTrigger = false;
        }
    }
}
