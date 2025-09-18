using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Item")]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public int itemID = 0;
    public Sprite icon;
    [TextArea(3, 10)]
    public string description;
    public bool isStackable = true;
    public int stackSize = 99;
    public GameObject prefab;
    public bool isWeapon = false;

    //[HideInInspector] public Transform weaponTransform; // Runtime-reference til våbenet
}
