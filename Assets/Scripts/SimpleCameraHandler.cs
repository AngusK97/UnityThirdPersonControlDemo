using UnityEngine;

/// <summary>
/// 使用 Cinemachine，不使用该脚本控制相机！
/// </summary>
public class SimpleCameraHandler : MonoBehaviour
{
    [Header("Mouse Input")]
    // 鼠标输入原始数据
    public Vector2 mouseInput;
    public float mouseInputX;
    public float mouseInputY;
    // 旋转角度
    public float mX;
    public float mY;
    // 计算后的相机最终角度与位置
    public Quaternion mRotation;
    public Vector3 mPosition;
    
    [Header("Camera Control Settings")]
    // 跟随目标
    public Transform targetToFollow;
    // 俯仰角度限制
    public float MinLimitY = -90;
    public float MaxLimitY = 90;
    // 距离目标的水平距离
    public float Distance = 10f;
    // 鼠标灵敏度
    public float mouseSensitivity = 2f;

    private PlayerControls _playerControls;

    
    //-----------------------------------------------------------------------------------------------
    // Lifecycle
    //-----------------------------------------------------------------------------------------------

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Camera.MouseLook.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
    }

    private void Start()
    {
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void LateUpdate()
    {
        UpdateMouseLook();
    }
    
    
    //-----------------------------------------------------------------------------------------------
    // MouseLook
    //-----------------------------------------------------------------------------------------------

    private void UpdateMouseLook()
    {
        // 获取鼠标输入原始数据 float
        mouseInputX = mouseInput.x;
        mouseInputY = mouseInput.y;
        // 处理鼠标输入数据
        mX += mouseInputX * mouseSensitivity * Time.fixedDeltaTime;
        mY -= mouseInputY * mouseSensitivity * Time.fixedDeltaTime;  // 由于鼠标向下滑，视角变为俯视角，所以 mY 需要 -=
        // 限制俯仰范围
        mY = ClampAngle(mY, MinLimitY, MaxLimitY);
        
        // 计算最终角度和位置
        mRotation = Quaternion.Euler(mY, mX, 0);
        mPosition = mRotation * new Vector3(0.0f, 2.0f, -Distance) + targetToFollow.position;
        
        // 通过 transform 设置相机的角度和位置
        transform.rotation = mRotation;
        transform.position = mPosition;
    }
    
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
