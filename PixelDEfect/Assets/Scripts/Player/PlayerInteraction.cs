using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private KeyCode input = KeyCode.G;

    private ButtonBase button; //가까운 버튼
    private DoorBase door; // 가까운 문

    private void Update()
    {
        if(Input.GetKeyDown(input) && button!= null)
        {
            button.ButtonTrigger();
            Debug.Log("버튼을 클릭");
        }
        else if(Input.GetKeyDown(input) && door != null)
        {
            door.ActiveDoor(gameObject);
            Debug.Log("문을 사용");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Button"))
        {
            button = collision.GetComponent<ButtonBase>();
        }
        else if (collision.CompareTag("Door"))
        {
            door = collision.GetComponent<DoorBase>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Button"))
        {
            button = null;
        }
        else if(collision.CompareTag("Door"))
        {
            door = null;
        }
    }
}
