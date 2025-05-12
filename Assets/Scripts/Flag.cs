using FishNet.Object;
using UnityEngine;
using FishNet.Object.Synchronizing;

public class Flag : NetworkBehaviour
{

    [ShowOnly][SerializeField] private Vector3 defaultSpawnPosition;
    private readonly SyncVar<bool> _isPickedUp = new();
    [SerializeField] private Renderer[] flagRenderer;
    private Collider _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        defaultSpawnPosition = transform.position;
        
        SetVisibility(true);
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
        transform.position = position;
        _isPickedUp.Value = false;
        SetVisibility(true);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void FlagReset()
    {
        //========== Flag Reset ==========//
        transform.position = defaultSpawnPosition;
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
}
