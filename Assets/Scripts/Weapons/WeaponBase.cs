using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public string weaponName; // Navn p� v�ben

    public abstract void Use(); // Ens funktion for alle v�ben
}
