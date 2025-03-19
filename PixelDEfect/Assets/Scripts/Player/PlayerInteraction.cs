using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private KeyCode input = KeyCode.G;

    private ButtonBase button;

    private void Update()
    {
        if(Input.GetKeyDown(input) && button!= null)
        {
            button.ButtonTrigger();
            Debug.Log("버튼을 클릭");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Button"))
        {

            button = collision.GetComponent<ButtonBase>();
        }
    }
}
