using FishNet.Object;
using UnityEngine;

public class SpawnBase : NetworkBehaviour
{
    public enum TeamColor
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }
    
    public TeamColor teamColor;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || 
            !other.TryGetComponent(out PlayerController playerController) || 
            !playerController.IsOwner || 
            !playerController.isHoldingFlag)
            return;
        
        playerController.DepositFlag();

    }
}
