using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Direction Calculation")]
    public Vector3 cameraStraightForward;
    public Vector3 cameraStraightRight;
    public Vector3 directionToLook;
    
    [Header("Move")]
    public bool leftShiftInput;
    public Vector2 moveInput;
    public float acceleration = 0.05f;
    public float deceleration = 0.1f;
    public float curSpeed;
    public float targetSpeed;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotateSpeed = 5f;
    public Vector3 velocity;
    
    [Header("Jump")]
    public Rigidbody playerRigidbody;
    public float jumpForce = 7f;
    public Vector3 upForce;

    [Header("Ground Detection")]
    public bool isGrounded;
    public bool isOnSlop;
    public float playerHeight = 2f;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Transform _transform;
    private Transform _mainCameraTransform;
    private PlayerControls _playerControls;


    //-----------------------------------------------------------------------------------------------
    // Lifecycle
    //-----------------------------------------------------------------------------------------------

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Locomotion.Movement.performed += ReadMovementInput;
        _playerControls.Locomotion.LeftShift.started += SetLeftShiftPressed;
        _playerControls.Locomotion.LeftShift.canceled += SetLeftShiftUnpressed;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _transform = transform;
        _mainCameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    void FixedUpdate()
    {
        isGrounded = CheckIsGrounded();
        isOnSlop = CheckIsOnSlop();
        
        if (isGrounded || isOnSlop)
        {
            if (moveInput.magnitude != 0)
            {
                RotatePlayer();
                MovePlayerForward();
            }
            else
            {
                if (curSpeed > 0)
                {
                    curSpeed -= deceleration * Time.fixedDeltaTime;
                    curSpeed = Mathf.Clamp(curSpeed, 0, float.MaxValue);   
                }
            }

            Jump();
        }

        velocity = playerRigidbody.velocity;
    }
    
    
    //-----------------------------------------------------------------------------------------------
    // Read Input System Data
    //-----------------------------------------------------------------------------------------------

    private void ReadMovementInput(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void SetLeftShiftPressed(InputAction.CallbackContext ctx)
    {
        leftShiftInput = true;
    }

    private void SetLeftShiftUnpressed(InputAction.CallbackContext ctx)
    {
        leftShiftInput = false;
    }


    //-----------------------------------------------------------------------------------------------
    // Locomotion
    //-----------------------------------------------------------------------------------------------
    
    private void RotatePlayer()
    {
        // 计算相机水平正前方向、水平正右方向
        cameraStraightForward = Vector3.Scale(_mainCameraTransform.forward, new Vector3(1, 0, 1));
        cameraStraightRight = Vector3.Scale(_mainCameraTransform.right, new Vector3(1, 0, 1));
        var position = _mainCameraTransform.position;
        Debug.DrawRay(position, cameraStraightRight * 10, Color.red);
        Debug.DrawRay(position, cameraStraightForward * 10, Color.blue);

        // 计算角色朝向
        directionToLook = Vector3.zero;
        if (moveInput.y > 0)
        {
            directionToLook += cameraStraightForward;
        }
        else if (moveInput.y < 0)
        {
            directionToLook += -cameraStraightForward;
        }
        
        if (moveInput.x > 0)
        {
            directionToLook += cameraStraightRight;
        }
        else if (moveInput.x < 0)
        {
            directionToLook += -cameraStraightRight;
        }
        directionToLook.Normalize();
        
        // 计算目标方向与角色前方方向的夹角度数
        var playerForward = _transform.forward;
        var desiredRotationAngle = Vector3.Angle(playerForward, directionToLook);

        // 使用叉乘，查看相机水平方向在角色当前正前方方向的左边还是右边
        // Unity 遵循左手螺旋定则，如果结果为正，说明相机方向在角色方向右边，使用正角度值进行旋转
        // 如果结果为负，说明相机方向在角色方向左边，使用负角度值进行旋转
        var crossProduct = Vector3.Cross(playerForward, directionToLook).y;
        if (crossProduct < 0)
        {
            desiredRotationAngle *= -1;
        }
        
        // 旋转角色
        var rotation = _transform.rotation;
        var newEulerRotation = rotation.eulerAngles + new Vector3(0, desiredRotationAngle);
        var newQuaternionRotation = Quaternion.Euler(newEulerRotation);
        rotation = Quaternion.Slerp(rotation, newQuaternionRotation, Time.deltaTime * rotateSpeed);
        _transform.rotation = rotation;
    }

    private void MovePlayerForward()
    {
        targetSpeed = leftShiftInput ? runSpeed : walkSpeed;
        if (curSpeed < targetSpeed)
        {
            curSpeed += acceleration * Time.fixedDeltaTime;
            curSpeed = Mathf.Clamp(curSpeed, 0, targetSpeed);
        }
        else if (curSpeed > targetSpeed)
        {
            curSpeed -= deceleration * Time.fixedDeltaTime;
            curSpeed = Mathf.Clamp(curSpeed, targetSpeed, float.MaxValue);
        }

        playerRigidbody.velocity = _transform.forward * curSpeed 
                                   + new Vector3(0f, playerRigidbody.velocity.y, 0f);
    }

    private void Jump()
    {
        if (_playerControls.Locomotion.Jump.triggered)
        {
            var curVelocity = playerRigidbody.velocity;
            playerRigidbody.velocity = new Vector3(curVelocity.x, 0f, curVelocity.z);
            
            upForce = _transform.up * jumpForce;
            playerRigidbody.AddForce(upForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }


    //-----------------------------------------------------------------------------------------------
    // Ground Detection
    //-----------------------------------------------------------------------------------------------
    
    private bool CheckIsGrounded()
    {
        var playerBottom = _transform.position - new Vector3(0, playerHeight / 2, 0);
        return Physics.CheckSphere(playerBottom, groundDistance, groundMask);
        // Note:
        // 1.记得在编辑器中新增一个 Layer: GroundLayer
        // 2.将 groundMask 设置为 GroundLayer
        // 3.将需要视为地面的物体的 Layer 都设置为 GroundLayer
    }

    private bool CheckIsOnSlop()
    {
        if (Physics.Raycast(_transform.position, Vector3.down,
            out var slopHit, playerHeight / 2 + 0.5f))
        {
            if (slopHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }
}
