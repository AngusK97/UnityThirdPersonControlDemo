using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public PlayerControls playerControls;
    
    public Vector2 moveVec;
    public bool leftShift;
    public bool attack;
    public bool jump;
    
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Start()
    {
        playerControls.Locomotion.Movement.performed += ReadMovementInput;
        playerControls.Locomotion.LeftShift.started += SetLeftShiftPressed;
        playerControls.Locomotion.LeftShift.canceled += SetLeftShiftUnpressed;
    }

    private void Update()
    {
        attack = playerControls.Attack.LightAttack.triggered;
        jump = playerControls.Locomotion.Jump.triggered;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    
    //-----------------------------------------------------------------------------------------------
    // Read Input System Data
    //-----------------------------------------------------------------------------------------------
    
    private void ReadMovementInput(InputAction.CallbackContext ctx)
    {
        moveVec = ctx.ReadValue<Vector2>();
    }

    private void SetLeftShiftPressed(InputAction.CallbackContext ctx)
    {
        leftShift = true;
    }

    private void SetLeftShiftUnpressed(InputAction.CallbackContext ctx)
    {
        leftShift = false;
    }
}
