using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("조작키")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode holdKey = KeyCode.F;
    [SerializeField] private KeyCode interactKey = KeyCode.G;
    [SerializeField] private KeyCode crouchKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode pickupKey = KeyCode.F;
    [SerializeField] private KeyCode meleeAttackKey = KeyCode.Z;
    [SerializeField] private KeyCode throwWeaponKey = KeyCode.X;

    private bool canPickup = false;
    private bool canHold = false;
    
    public float HorizontalInput => Input.GetAxisRaw("Horizontal");
    public float VerticalInput => Input.GetAxisRaw("Vertical");
    public float SprintInput => Input.GetAxisRaw("Sprint");

    public event Action OnJumpPressed;
    public event Action OnCrouchPressed;
    public event Action OnCrouchReleased;
    public event Action OnHoldPressed;
    public event Action OnInteractPressed;
    public event Action OnValvePressed;
    public event Action OnValveReleased;
    public event Action OnLadderJumpPressed;
    public event Action OnPickupPressed;
    public event Action OnMeleeAttackPressed;
    public event Action OnThrowWeaponPressed;

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
        
        // 웅크리기 입력
        if(IsCrouchKeyPressed())
            OnCrouchPressed?.Invoke();
        if(Input.GetKeyUp(crouchKey))
            OnCrouchReleased?.Invoke();
            
        // 끌기 입력
        if (Input.GetKeyDown(holdKey) && canHold)
            OnHoldPressed?.Invoke();
        
        // 상호작용 입력
        if (Input.GetKeyDown(interactKey))
            OnInteractPressed?.Invoke();
        
        // 밸브 입력
        if (Input.GetKey(interactKey))
            OnValvePressed?.Invoke();
        if (Input.GetKeyUp(interactKey))
            OnValveReleased?.Invoke();
            

        // 공격 입력
        if (Input.GetKeyDown(pickupKey) && canPickup)
            OnPickupPressed?.Invoke();
        if (Input.GetKeyDown(meleeAttackKey))
            OnMeleeAttackPressed?.Invoke();

        // 무기 던지기 입력
        if (Input.GetKeyDown(throwWeaponKey))
            OnThrowWeaponPressed?.Invoke();
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
    
    public bool IsCrouchKeyPressed() => Input.GetKey(crouchKey);
    
    public void SetCanPickup(bool value) => canPickup = value;
    public void SetCanHold(bool value) => canHold = value;
}
