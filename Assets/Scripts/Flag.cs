using FishNet.Object;
using UnityEngine;
using FishNet.Object.Synchronizing;

public class Flag : NetworkBehaviour
{

    private Vector3 _defaultSpawnPosition;
    private readonly SyncVar<Vector3> _currentPosition = new();
    private readonly SyncVar<bool> _isPickedUp = new();
    private readonly SyncVar<int> _currentTeamIDHoldingFlag = new();
    [SerializeField] private Renderer[] flagRenderer;
    private Collider _collider;
    
    
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly][SerializeField] private bool isPickedUp;
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly] public int currentTeamIDHoldingFlag;
    
    [SerializeField] private SpawnBase spawnBase;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _defaultSpawnPosition = transform.position;
        _currentPosition.Value = _defaultSpawnPosition;
        SetVisibility(true);
        
        // Subscribe to the OnChange event
        _isPickedUp.OnChange += OnFlagPickedUpChanged;
        _currentPosition.OnChange += OnFlagPositionChanged;
        _currentTeamIDHoldingFlag.OnChange += OnFlagHolderTeamIDChanged;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from the OnChange event to avoid memory leaks
        _isPickedUp.OnChange -= OnFlagPickedUpChanged;
        _currentPosition.OnChange -= OnFlagPositionChanged;
        _currentTeamIDHoldingFlag.OnChange -= OnFlagHolderTeamIDChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FlagPickedUp(int teamID)
    {
        //========== Flag Pickup ==========//
        if (_isPickedUp.Value) return;
        
        _isPickedUp.Value = true;
        _currentTeamIDHoldingFlag.Value = teamID;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FlagDropped(Vector3 position)
    {
        //========== Flag Drop ==========//
        position.y -= 1.08f; // Players are 1.08 units tall
        _currentPosition.Value = position;
        _isPickedUp.Value = false;
        _currentTeamIDHoldingFlag.Value = 0;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FlagReset()
    {
        //========== Flag Reset ==========//
        _currentPosition.Value = _defaultSpawnPosition;
        _isPickedUp.Value = false;
        _currentTeamIDHoldingFlag.Value = 0;
    }
    
    private void SetVisibility(bool isVisible)
    {
        foreach (var render in flagRenderer)
        {
            render.enabled = isVisible;
        }
        
        _collider.enabled = isVisible;
    }
    
    #region SyncVar Update
    
    // ========== Flag Update ========== //
    private void OnFlagPickedUpChanged(bool previous, bool current, bool asServer)
    {
        if (asServer) return;
        
        isPickedUp = current;
        SetVisibility(!current);
        Debug.Log(current ? "Flag picked up" : "Flag dropped");
    }
    
    private void OnFlagPositionChanged(Vector3 previous, Vector3 current, bool asServer)
    {
        if (asServer) return;
        
        transform.position = current;
        Debug.Log("Flag position changed to: " + current);
    }

    private void OnFlagHolderTeamIDChanged(int previous, int current, bool asServer)
    {
        if (asServer) return;
        currentTeamIDHoldingFlag = current;
    }
    
    #endregion
}
