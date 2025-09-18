using UnityEngine;
using System;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{

    [Header("Våben i hierarkiet")]
    public List<WeaponEntry> weapons = new List<WeaponEntry>();

    public void ActivateWeapon(bool isActive, string weaponName = null)
    {
        string weaponID = weaponName;
        foreach (var weapon in weapons)
        {
            if (weapon.weaponName == weaponID && isActive)
            {
                weapon.weaponObject.SetActive(true);
                Debug.Log($"Aktiverede våben: {weaponID}");
            }
            else 
            {
                weapon.weaponObject.SetActive(false); // Deaktiver andre våben
            }
        }
    }
}
