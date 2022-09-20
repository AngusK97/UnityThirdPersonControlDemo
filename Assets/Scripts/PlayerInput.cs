using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 moveVec;
    public bool leftShift;
    public bool attack;
    public bool jump;
    
    private PlayerControls _playerControls;
    
    private void Awake()
    {
        _playerControls = new PlayerControls();
    }
    
    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void Start()
    {
        _playerControls.Locomotion.Movement.performed += ReadMovementInput;
        _playerControls.Locomotion.LeftShift.started += SetLeftShiftPressed;
        _playerControls.Locomotion.LeftShift.canceled += SetLeftShiftUnpressed;
    }

    private void Update()
    {
        attack = _playerControls.Attack.LightAttack.triggered;
        jump = _playerControls.Locomotion.Jump.triggered;
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

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
