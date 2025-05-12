using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class Weapons : NetworkBehaviour
{
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    private Weapon _currentWeapon;

    private int _teamID;
    
    private PlayerInfo _playerInfo;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner) return;
        enabled = false;
    }
    
    private void Awake()
    {
        if (TryGetComponent(out _playerInfo))
        {
            //_teamID = _playerInfo.teamID.Value;
        }
    }

    public void InitializeWeapon( Transform parentTransform)
    {
        foreach (var weapon in weapons.Where(weapon => weapon != null))
        {
            weapon.transform.SetParent(parentTransform);
        }
    }
    
    /// <summary>
    /// Weapon Color
    /// - Red = 0 = Team 1
    /// - Blue = 1 = Team 2
    /// - Green = 2 = Team 3
    /// - Yellow = 3 = Team 4
    /// </summary>

    public void AssignWeapon()
    {
        switch (_teamID)
        {
            case 1:
                _currentWeapon = weapons[0];
                weapons[0].gameObject.SetActive(true);
                Debug.Log("Assign Red team weapon");
                break;
            case 2:
                _currentWeapon = weapons[1];
                weapons[1].gameObject.SetActive(true);
                Debug.Log("Assign Blue team weapon");
                break;
            case 3:
                _currentWeapon = weapons[2];
                weapons[2].gameObject.SetActive(true);
                Debug.Log("Assign Green team weapon");
                break;
            case 4:
                _currentWeapon = weapons[3];
                weapons[3].gameObject.SetActive(true);
                Debug.Log("Assign Yellow team weapon");
                break;
            default:
                Debug.LogWarning("Assign weapon logic error");
                _currentWeapon = weapons[0];
                weapons[0].gameObject.SetActive(true);
                break;
        }
    }

    public void OnFire()
    {
        Debug.Log("OnFire");
        //_currentWeapon.Fire();
    }
}
