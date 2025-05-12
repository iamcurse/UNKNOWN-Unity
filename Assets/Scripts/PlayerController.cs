// using UnityEngine;
// using FishNet.Object;
// using UnityEngine.InputSystem;
//  
// public class PlayerController : NetworkBehaviour
// {
//     private InputSystem_Actions _playerInput;
//     private InputAction _move;
//     private InputAction _look;
//     private InputAction _jump;
//     private InputAction _sprint;
//     private InputAction _fire;
//     
//     private Weapons _weapons;
//     
//     [Header("Base setup")]
//     public float walkingSpeed = 7.5f;
//     public float runningSpeed = 11.5f;
//     public float jumpSpeed = 8.0f;
//     public float gravity = 20.0f;
//     public float lookSpeed = 2.0f;
//     public float lookXLimit = 45.0f;
//
//     private CharacterController _characterController;
//     private Vector3 _moveDirection = Vector3.zero;
//     private float _rotationX;
//  
//     [HideInInspector]
//     public bool canMove = true;
//  
//     [SerializeField]
//     private float cameraYOffset = 0.4f;
//     private Camera _playerCamera;
//     
//     public override void OnStartClient()
//     {
//         base.OnStartClient();
//         if (IsOwner)
//         {
//             _playerCamera = Camera.main;
//             if (_playerCamera == null) return;
//             _playerCamera.transform.position = new Vector3(transform.position.x,
//                 transform.position.y + cameraYOffset, transform.position.z);
//             _playerCamera.transform.SetParent(transform);
//
//             //========== Sync Player Weapon Transform ==========
//             if (TryGetComponent(out Weapons weapons))
//             {
//                 weapons.InitializeWeapon(_playerCamera.transform);
//             }
//             else
//             {
//                 Debug.LogError("PlayerWeapon component not found on this GameObject.");
//             }
//         }
//         else
//         {
//             gameObject.GetComponent<PlayerController>().enabled = false;
//         }
//     }
//
//     private void Awake()
//     {
//
//         if (TryGetComponent(out Weapons weapons))
//         {
//             _weapons = weapons;
//         }
//         else
//         {
//             Debug.LogError("PlayerWeapon component not found on this GameObject.");
//         }
//     }
//
//     private void Start()
//     {
//         _characterController = GetComponent<CharacterController>();
//  
//         // Lock cursor
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }
//  
//     private void Update()
//     {
//         // Press Left Shift to run
//         var isRunning = Input.GetKey(KeyCode.LeftShift);
//
//         // We are grounded, so recalculate move direction based on axis
//         var forward = transform.TransformDirection(Vector3.forward);
//         var right = transform.TransformDirection(Vector3.right);
//  
//         var curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
//         var curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
//         var movementDirectionY = _moveDirection.y;
//         _moveDirection = ((forward * curSpeedX) + (right * curSpeedY)).normalized * (isRunning ? runningSpeed : walkingSpeed);
//  
//         if (Input.GetButton("Jump") && canMove && _characterController.isGrounded)
//         {
//             _moveDirection.y = jumpSpeed;
//         }
//         else
//         {
//             _moveDirection.y = movementDirectionY;
//         }
//  
//         if (!_characterController.isGrounded)
//         {
//             _moveDirection.y -= gravity * Time.deltaTime;
//         }
//  
//         // Move the controller
//         _characterController.Move(_moveDirection * Time.deltaTime);
//  
//         // Player and Camera rotation
//         if (!canMove || _playerCamera == null) return;
//         _rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
//         _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
//         _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
//         transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
//     }
//
// }
//  

using UnityEngine;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
    private InputSystem_Actions _playerInput;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isJumping;
    private bool _isSprinting;

    private Weapons _weapons;

    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 0.2f;
    public float lookXLimit = 45.0f;

    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera _playerCamera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            _playerCamera = Camera.main;
            if (_playerCamera == null) return;
            _playerCamera.transform.position = new Vector3(transform.position.x,
                transform.position.y + cameraYOffset, transform.position.z);
            _playerCamera.transform.SetParent(transform);

            if (TryGetComponent(out Weapons weapons))
                weapons.InitializeWeapon(_playerCamera.transform);

            // Enable Input
            _playerInput = new InputSystem_Actions();
            _playerInput.Enable();
            RegisterInput();
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    private void Awake()
    {
        if (TryGetComponent(out Weapons weapons))
            _weapons = weapons;
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RegisterInput()
    {
        _playerInput.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _playerInput.Player.Move.canceled += _ => _moveInput = Vector2.zero;

        _playerInput.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _playerInput.Player.Look.canceled += _ => _lookInput = Vector2.zero;

        _playerInput.Player.Jump.performed += _ => _isJumping = true;
        _playerInput.Player.Jump.canceled += _ => _isJumping = false;

        _playerInput.Player.Sprint.performed += _ => _isSprinting = true;
        _playerInput.Player.Sprint.canceled += _ => _isSprinting = false;

        // Add fire callback if needed:
        // _playerInput.Player.Fire.performed += ctx => FireWeapon();
    }

    private void Update()
    {
        if (!IsOwner) return;

        var forward = transform.TransformDirection(Vector3.forward);
        var right = transform.TransformDirection(Vector3.right);

        float curSpeed = canMove ? (_isSprinting ? runningSpeed : walkingSpeed) : 0;
        Vector3 movement = (forward * _moveInput.y + right * _moveInput.x).normalized * curSpeed;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = movement;
        _moveDirection.y = movementDirectionY;

        if (_characterController.isGrounded && canMove && _isJumping)
        {
            _moveDirection.y = jumpSpeed;
        }
        else if (!_characterController.isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);

        if (!canMove || _playerCamera == null) return;

        _rotationX += -_lookInput.y * lookSpeed;
        _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, _lookInput.x * lookSpeed, 0);    }

    private void OnDisable()
    {
        _playerInput?.Disable();
    }
}
