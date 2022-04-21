using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 接收 InputSystem 的输入信息，并驱动功能
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public Transform mainCameraTransform;
    
    [Header("Raw Input Data")]
    public Vector2 moveInput;
    public Vector2 cameraInput;
    public bool isIntensifyPressed;
    public bool isSpacePressed;
        
    private PlayerControls _playerInput;
    private Vector3 _moveVelocity;


    //-----------------------------------------------------------------------------------------------
    // 生命周期
    //-----------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        if (_playerInput == null)
        {
            _playerInput = new PlayerControls();
                
            // 持续监控
            _playerInput.Locomotion.Movement.performed += HandleMovementInput;
            // _playerInput.Locomotion.Camera.performed += HandleCameraInput;
                
            // 长按
            // _playerInput.Locomotion.Intensify.started += EnableIntensify;
            // _playerInput.Locomotion.Intensify.canceled += DisableIntensify;
                
            // 单击
            // _playerInput.CharacterMovement.Space.started += SetSpacePressed;
        }
            
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Disable();
    }

    private void Update()
    {
        // UpdateMovement();
    }


    //-----------------------------------------------------------------------------------------------
    // 重置单击 flags
    //-----------------------------------------------------------------------------------------------

    public void LateUpdate()
    {
        // SetSpaceUnpressed();
    }


    //-----------------------------------------------------------------------------------------------
    // 输入回调
    //-----------------------------------------------------------------------------------------------

    private void HandleMovementInput(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        // [1, 0]
    }

    // private void HandleCameraInput(InputAction.CallbackContext ctx)
    // {
    //     cameraInput = ctx.ReadValue<Vector2>();
    // }
    //
    // private void EnableIntensify(InputAction.CallbackContext ctx)
    // {
    //     isIntensifyPressed = true;
    // }
    //
    // private void DisableIntensify(InputAction.CallbackContext ctx)
    // {
    //     isIntensifyPressed = false;
    // }
    //
    // private void SetSpacePressed(InputAction.CallbackContext ctx)
    // {
    //     isSpacePressed = true;
    //     Debug.LogError($"SetSpacePressed, _isSpacePressed = true, Jump!");
    // }
    //
    // private void SetSpaceUnpressed()
    // {
    //     isSpacePressed = false;
    // }
        
        
    //-----------------------------------------------------------------------------------------------
    // 更新角色功能所需数据
    //-----------------------------------------------------------------------------------------------

    // private void UpdateMovement()
    // {
    //     // direction
    //     _moveVelocity = mainCameraTransform.forward * moveInput.y;
    //     _moveVelocity += mainCameraTransform.right * moveInput.x;
    //     _moveVelocity.y = 0;
    //     _moveVelocity.Normalize();
    //     Debug.LogError($"normalized move velocity = {_moveVelocity}");
    //     
    //     // speed
    //     _moveVelocity = _moveVelocity * moveSpeed * Time.deltaTime;
    //     Debug.LogError($"final move velocity = {_moveVelocity}");
    //     characterController.Move(_moveVelocity);
    // }
}