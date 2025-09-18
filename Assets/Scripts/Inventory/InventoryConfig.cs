using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfig", menuName = "Inventory/Config")]
public class InventoryConfig : ScriptableObject
{
    public float holdDistance = 1.5f;
    public float smoothTime = 0.1f;
}
