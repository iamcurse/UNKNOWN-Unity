using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{

    private Tugboat _tugboat;

    #region Client Disconnection
    
    /// ========== Client Lost Connection To Server ==========
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }
    
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        if (obj.ConnectionState != LocalConnectionState.Stopping) return;
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Stopping play mode due to lost connection to the server.");
    }
    
    #endregion
    
    #region Connection Process
    
    /// ========== Start Connection Process ==========
    private void Start() {

        if (TryGetComponent(out Tugboat tugboat))
        {
            _tugboat = tugboat;
        }
        else
        {
            Debug.LogError("Tugboat component not found on this GameObject.");
            return;
        }
        
        // Start the connection process
        if (ParrelSync.ClonesManager.IsClone())
        {
            _tugboat.StartConnection(false);
        }
        else
        {
            _tugboat.StartConnection(true);
            _tugboat.StartConnection(false);
        }
    }
    
    #endregion
}
