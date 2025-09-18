using UnityEngine;

public enum WeaponType { Ranged, Melee }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public float attackDamage;
    public float attackSpeed;
    public float range;

    // Gravity Gun-specifikke værdier
    public float attractionForce;
    public float throwForce;
}
