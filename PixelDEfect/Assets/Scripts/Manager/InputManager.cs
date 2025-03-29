using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("조작키")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode holdKey = KeyCode.F;
    [SerializeField] private KeyCode interactKey = KeyCode.G;
    [SerializeField] private KeyCode healKey = KeyCode.E;
    
    public float HorizontalInput => Input.GetAxisRaw("Horizontal");
    public float VerticalInput => Input.GetAxisRaw("Vertical");
    public float SprintInput => Input.GetAxisRaw("Sprint");

    public event Action OnJumpPressed;
    public event Action OnHoldPressed;
    public event Action OnInteractPressed;
    public event Action OnHealPressed;
    public event Action OnLadderJumpPressed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        CheckInputs();
        CheckKeyCombinations();
    }

    private void CheckInputs()
    {
        // 점프 입력
        if (Input.GetKeyDown(jumpKey))
            OnJumpPressed?.Invoke();
        
        // 끌기 입력
        if (Input.GetKeyDown(holdKey))
            OnHoldPressed?.Invoke();
        
        // 상호작용 입력
        if (Input.GetKeyDown(interactKey))
            OnInteractPressed?.Invoke();
        
        if (Input.GetKeyDown(healKey))
            OnHealPressed?.Invoke();
    }
    
    private void CheckKeyCombinations()
    {
        // 사다리 점프 입력
        if (HorizontalInput != 0)
        {
            if (Input.GetKeyDown(jumpKey))
                OnLadderJumpPressed?.Invoke();
        }
    }
}
