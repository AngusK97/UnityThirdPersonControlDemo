using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActorController : MonoBehaviour
{
    public Animator anim;
    public Transform model;
    public PlayerInput pi;

    [Header("Direction Calculation")]
    public Vector3 cameraStraightForward;
    public Vector3 cameraStraightRight;
    public Vector3 bodyTargetDirection;
    
    [Header("Move")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotateSpeed = 5f;
    public float acceleration = 0.05f;
    public float deceleration = 0.1f;
    public float targetSpeed;
    public float curSpeed;
    public Vector3 velocity;
    
    [Header("Jump")]
    public Rigidbody rigid;
    public float jumpForce = 7f;
    public Vector3 upForce;

    [Header("Drag")]
    public float landDrag = 1f;
    public float airDrag = 0f;

    [Header("Ground Detection")]
    public bool isOnGround;
    public bool isOnSlop;
    public float playerHeight = 2f;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Head Movement")]
    public Transform head;
    public Transform lookPoint;
    public float lookPointDistance = 5f;

    [Header("Anim Params")]
    [SerializeField] private string speedParam;
    [SerializeField] private string jumpParam;
    [SerializeField] private string isOnGroundedParam;
    [SerializeField] private string attackParam;
    private int _speedParamHash;
    private int _jumpParamHash;
    private int _isGroundedParamHash;
    private int _attackParamHash;
    
    private bool _isAttacking;
    private Transform _transform;
    private Transform _mainCameraTransform;


    //-----------------------------------------------------------------------------------------------
    // Lifecycle
    //-----------------------------------------------------------------------------------------------

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _transform = transform;
        _mainCameraTransform = Camera.main.transform;
        _speedParamHash = Animator.StringToHash(speedParam);
        _jumpParamHash = Animator.StringToHash(jumpParam);
        _isGroundedParamHash = Animator.StringToHash(isOnGroundedParam);
        _attackParamHash = Animator.StringToHash(attackParam);
    }

    void FixedUpdate()
    {
        // 计算相机水平正前方向、水平正右方向
        cameraStraightForward = Vector3.Scale(_mainCameraTransform.forward, new Vector3(1, 0, 1));
        cameraStraightRight = Vector3.Scale(_mainCameraTransform.right, new Vector3(1, 0, 1));
        var position = _mainCameraTransform.position;
        Debug.DrawRay(position, cameraStraightRight * 10, Color.red);
        Debug.DrawRay(position, cameraStraightForward * 10, Color.blue);

        isOnGround = CheckIsGrounded();
        isOnSlop = CheckIsOnSlop();
        
        if (pi.attack && isOnGround)
            anim.SetTrigger(_attackParamHash);
        
        if (isOnGround || isOnSlop)
        {
            rigid.drag = landDrag;
            
            anim.SetBool(_isGroundedParamHash, true);
            
            if (!_isAttacking)
            {
                if (pi.moveVec.magnitude != 0)
                {
                    RotatePlayerBody();
                    MovePlayerForward();
                }
                else
                {
                    if (curSpeed > 0)
                    {
                        curSpeed -= deceleration * Time.fixedDeltaTime;
                        curSpeed = Mathf.Clamp(curSpeed, 0, float.MaxValue);
                        anim.SetFloat(_speedParamHash, curSpeed);
                    }
                }
                
                Jump();
            }
            else
            {
                RotateAttackingPlayerBody();
                MoveAttackingPlayerForward();
            }
        }
        else
        {
            rigid.drag = airDrag;
            anim.SetBool(_isGroundedParamHash, false);
        }
        
        UpdateLookPointPosition();

        velocity = rigid.velocity;
    }


    //-----------------------------------------------------------------------------------------------
    // Locomotion
    //-----------------------------------------------------------------------------------------------
    
    private void RotatePlayerBody()
    {
        // 计算角色朝向
        bodyTargetDirection = Vector3.zero;
        if (pi.moveVec.y > 0)
        {
            bodyTargetDirection += cameraStraightForward;
        }
        else if (pi.moveVec.y < 0)
        {
            bodyTargetDirection += -cameraStraightForward;
        }
        
        if (pi.moveVec.x > 0)
        {
            bodyTargetDirection += cameraStraightRight;
        }
        else if (pi.moveVec.x < 0)
        {
            bodyTargetDirection += -cameraStraightRight;
        }
        bodyTargetDirection.Normalize();
        
        // 计算目标方向与角色前方方向的夹角度数
        var playerForward = _transform.forward;
        var desiredRotationAngle = Vector3.Angle(playerForward, bodyTargetDirection);

        // 使用叉乘，查看相机水平方向在角色当前正前方方向的左边还是右边
        // Unity 遵循左手螺旋定则，如果结果为正，说明相机方向在角色方向右边，使用正角度值进行旋转
        // 如果结果为负，说明相机方向在角色方向左边，使用负角度值进行旋转
        var crossProduct = Vector3.Cross(playerForward, bodyTargetDirection).y;
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
        targetSpeed = pi.leftShift ? runSpeed : walkSpeed;
        
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
        
        anim.SetFloat(_speedParamHash, curSpeed);
        rigid.velocity = _transform.forward * curSpeed + new Vector3(0f, rigid.velocity.y, 0f);
    }
    
    private void RotateAttackingPlayerBody()
    {
        // 计算角色朝向
        bodyTargetDirection = cameraStraightForward;
        bodyTargetDirection.Normalize();
        
        // 计算目标方向与角色前方方向的夹角度数
        var playerForward = _transform.forward;
        var desiredRotationAngle = Vector3.Angle(playerForward, bodyTargetDirection);

        // 使用叉乘，查看相机水平方向在角色当前正前方方向的左边还是右边
        // Unity 遵循左手螺旋定则，如果结果为正，说明相机方向在角色方向右边，使用正角度值进行旋转
        // 如果结果为负，说明相机方向在角色方向左边，使用负角度值进行旋转
        var crossProduct = Vector3.Cross(playerForward, bodyTargetDirection).y;
        if (crossProduct < 0) desiredRotationAngle *= -1;
        
        // 旋转角色
        var rotation = _transform.rotation;
        var newEulerRotation = rotation.eulerAngles + new Vector3(0, desiredRotationAngle);
        var newQuaternionRotation = Quaternion.Euler(newEulerRotation);
        rotation = Quaternion.Slerp(rotation, newQuaternionRotation, Time.deltaTime * rotateSpeed);
        _transform.rotation = rotation;
    }

    private void MoveAttackingPlayerForward()
    {
        targetSpeed = 2f;
        
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
        
        anim.SetFloat(_speedParamHash, curSpeed);
        rigid.velocity = _transform.forward * curSpeed + new Vector3(0f, rigid.velocity.y, 0f);
    }

    private void Jump()
    {
        if (pi.jump)
        {
            isOnGround = false;
            rigid.drag = airDrag;

            var curVelocity = rigid.velocity;
            rigid.velocity = new Vector3(curVelocity.x, 0f, curVelocity.z);
            
            upForce = _transform.up * jumpForce;
            rigid.AddForce(upForce, ForceMode.Impulse);
            
            anim.SetTrigger(_jumpParamHash);
            anim.SetBool(_isGroundedParamHash, false);
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
                return true;
        }
        return false;
    }
    
    
    //-----------------------------------------------------------------------------------------------
    // Head Movement
    //-----------------------------------------------------------------------------------------------

    public void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtWeight(1, 0, 1, 0, 0.50f);
        anim.SetLookAtPosition(lookPoint.position);
    }
    
    private void UpdateLookPointPosition()
    {
        // 判断视角与角色前方的同向、反向关系，并根据同/反方向确认观察点的最终位置
        var directionValue = Vector3.Dot(head.position - _mainCameraTransform.position, _transform.forward);
        Vector3 finalPointPosition;
        if (directionValue < 0)
        {
            finalPointPosition = _transform.forward * lookPointDistance + head.position;
        }
        else
        {
            finalPointPosition = cameraStraightForward * lookPointDistance + head.position;
        }
        
        // 插值得到观察点的新位置
        var curPointPosition = lookPoint.position;
        var nextPointPosition = Vector3.Slerp(curPointPosition, finalPointPosition, 0.05f);
        lookPoint.position = nextPointPosition;
    }
    
    
    //-----------------------------------------------------------------------------------------------
    // Anim message processing block
    //-----------------------------------------------------------------------------------------------

    private void OnAttackIdleEnter()
    {
        _isAttacking = false;
    }

    private void OnAttackIdleExit()
    {
        _isAttacking = true;
    }
}
