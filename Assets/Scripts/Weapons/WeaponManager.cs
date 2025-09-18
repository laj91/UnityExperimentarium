using UnityEngine;
using System;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{

    [Header("V�ben i hierarkiet")]
    public List<WeaponEntry> weapons = new List<WeaponEntry>();

    public void ActivateWeapon(bool isActive, string weaponName = null)
    {
        string weaponID = weaponName;
        foreach (var weapon in weapons)
        {
            if (weapon.weaponName == weaponID && isActive)
            {
                weapon.weaponObject.SetActive(true);
                Debug.Log($"Aktiverede v�ben: {weaponID}");
            }
            else 
            {
                weapon.weaponObject.SetActive(false); // Deaktiver andre v�ben
            }
        }
    }
}
