using FishNet.Object;
using Unity.VisualScripting.Dependencies.NCalc;
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
            !playerController.IsOwner)
            return;

        if (playerController.teamID == 0)
            playerController.teamID = (int)teamColor;
        
        if (playerController.isHoldingFlag)
            playerController.DepositFlag();

    }

}
