using FishNet.Object;
using UnityEngine;
using FishNet.Object.Synchronizing;

public class Flag : NetworkBehaviour
{

    private Vector3 _defaultSpawnPosition;
    private readonly SyncVar<Vector3> _currentPosition = new();
    private readonly SyncVar<bool> _isPickedUp = new();
    [SerializeField] private Renderer[] flagRenderer;
    private Collider _collider;
    
    
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly][SerializeField] private bool isPickedUp;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _defaultSpawnPosition = transform.position;
        
        SetVisibility(true);
        _isPickedUp.OnChange += OnFlagPickedUpChanged;
        _currentPosition.OnChange += OnFlagPositionChanged;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from the OnChange event to avoid memory leaks
        _isPickedUp.OnChange -= OnFlagPickedUpChanged;
        _currentPosition.OnChange -= OnFlagPositionChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void FlagPickedUp()
    {
        //========== Flag Pickup ==========//
        if (_isPickedUp.Value) return;
        
        _isPickedUp.Value = true;
        SetVisibility(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FlagDropped(Vector3 position)
    {
        //========== Flag Drop ==========//
        _currentPosition.Value = position;
        _isPickedUp.Value = false;
        SetVisibility(true);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FlagReset()
    {
        //========== Flag Reset ==========//
        _currentPosition.Value = _defaultSpawnPosition;
        _isPickedUp.Value = false;
        SetVisibility(true);
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
        transform.position = _currentPosition.Value;
        Debug.Log(current ? "Flag picked up" : "Flag dropped");
    }
    
    private void OnFlagPositionChanged(Vector3 previous, Vector3 current, bool asServer)
    {
        if (asServer) return;
        
        transform.position = current;
        Debug.Log("Flag position changed to: " + current);
    }
    
    #endregion
}
