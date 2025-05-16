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
            !playerController.IsOwner)
            return;

        // Set the player's team ID to the spawn base's team color
        if (playerController.GetTeamID() == 0)
        {
            playerController.SetTeamID((int)teamColor);
            Debug.Log("Set team to " + teamColor + " " + (int)teamColor);
        }
            
        // If the player is holding the flag and is on the same team as the spawn base, deposit the flag
        if (playerController.isHoldingFlag && playerController.GetTeamID() == (int)teamColor)
            playerController.DepositFlag();

    }

}
