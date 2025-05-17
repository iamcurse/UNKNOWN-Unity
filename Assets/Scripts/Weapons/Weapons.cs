using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class Weapons : NetworkBehaviour
{
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    [SerializeField] private List<GameObject> visors = new List<GameObject>();
    // ReSharper disable once NotAccessedField.Local
    [ShowOnly][SerializeField] private Weapon currentWeapon;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner) return;
        enabled = false;
    }

    [ServerRpc]
    public void AssignWeaponServerRPC(int teamID)
    {
        AssignWeapon(teamID);
    }

    public void InitializeWeapon(Transform parentTransform)
    {
        foreach (var weapon in weapons.Where(weapon => weapon != null))
        {
            weapon.transform.SetParent(parentTransform);
        }
    }
    
    // Weapon Color
    // - Red = 0 = Team 1
    // - Blue = 1 = Team 2
    // - Green = 2 = Team 3
    // - Yellow = 3 = Team 4

    [ObserversRpc]
    private void AssignWeapon(int teamID)
    {
        var weaponID = teamID - 1;
        
        currentWeapon = weapons[weaponID];
        
        foreach (var weapon in weapons.Where(w => w != null))
        {
            weapon.gameObject.SetActive(false);
        }

        foreach (var visor in visors)
        {
            visor.SetActive(false);          
        }
        
        if (weaponID >= 0 && weaponID < weapons.Count)
        {
            currentWeapon.gameObject.SetActive(true);
            visors[weaponID].SetActive(true);

            // Check if the weapon is active in the hierarchy
            StartCoroutine(ActivateWeaponDelayed(currentWeapon.gameObject, weaponID));

        }
        else
        {
            Debug.LogWarning("Weapon ID out of range: " + weaponID);
        }
    }
    
    private IEnumerator ActivateWeaponDelayed(GameObject weapon, int weaponID)
    {
        yield return new WaitForEndOfFrame(); // Give Unity a full frame to catch up
        
        if (weapon.gameObject.activeSelf) yield break;

        weapon.gameObject.SetActive(true);
        visors[weaponID].SetActive(true);
        Debug.LogWarning($"Weapon {currentWeapon.name} is not active in hierarchy. Forcing activation.");
    }

}
