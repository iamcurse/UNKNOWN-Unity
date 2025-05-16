using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerController : NetworkBehaviour
{
    
    private InputSystem_Actions _playerInput;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isJumping;
    private bool _isSprinting;
    
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

    #region Player Control
    
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

            StartCoroutine(DelayedInitialize());

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
    
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _captureTheFlag = FindAnyObjectByType<CaptureTheFlag>();
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

    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (TryGetComponent(out Weapons weapons))
            weapons.InitializeWeapon(_playerCamera.transform);
    }
    
    #endregion

    private void Awake()
    {
        if (FindAnyObjectByType<UIManager>())
        {
            _uiManager = FindAnyObjectByType<UIManager>();
        }
        else
        {
            Debug.LogError("No UIManager found in the scene.");
        }

        _teamID.OnChange += OnTeamIDChange;
    }

    #region Logic
    
    [Header("Game")]
    private readonly SyncVar<int> _teamID = new(-1);
    [ShowOnly][SerializeField] private int teamID;
    
    [SerializeField] private bool isDead;

    [ServerRpc]
    public void SetTeamID(int id)
    {
        _teamID.Value = id;
    }
    
    public int GetTeamID()
    {
        return teamID;
    }

    private void OnTeamIDChange(int oldValue, int newValue, bool asServer)
    {
        teamID = newValue;
    }
    
    private void OnDeath()
    {
        //isDead = true;
        
        DropFlag();
    }
    
    #endregion

    #region Flag

    private CaptureTheFlag _captureTheFlag;
    
    private Flag _flag;
    [ShowOnly] public bool isHoldingFlag;
    private UIManager _uiManager;
    
    // ========== Flag Pickup ========== //
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Flag") || !IsOwner) return;
        
        if (isHoldingFlag || isDead) return;
        
        _flag = other.GetComponent<Flag>();
        _flag.FlagPickedUp(teamID);
        isHoldingFlag = true;
        _uiManager.flagImage.enabled = true;
    }
    
    // ========== Flag Drop ========== //
    private void DropFlag()
    {
        if (!isDead || !isHoldingFlag) return;
        
        _flag.FlagDropped(transform.position);
        isHoldingFlag = false;
        _uiManager.flagImage.enabled = false;
    }
    
    public void DepositFlag()
    {
        if (!isHoldingFlag) return;
        
        _captureTheFlag.AddScore(_flag.currentTeamIDHoldingFlag);
        isHoldingFlag = false;
        _uiManager.flagImage.enabled = false;
        _flag.FlagReset();
    }
    
    #endregion

    public void OnFire()
    {
        if (!IsOwner) return;
        Debug.Log(teamID);
    }
    
}
