using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public string weaponName; // Navn på våben

    public abstract void Use(); // Ens funktion for alle våben
}
