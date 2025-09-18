using UnityEngine;

[CreateAssetMenu(menuName = "Objectives/Collect Item Objective")]
public class CollectItemObjectiveData : ObjectiveData
{
    public int amount; // Antal af det krævede item
    public Item itemPrefab; // Refererer til det item, der skal samles

    // Tilføj en property for at matche 'requiredAmount'
    public int requiredAmount => amount;

    public override Objective CreateRuntimeObjective()
    {
        // Opretter en runtime-instans af CollectItemObjective
        Inventory playerInventory = FindObjectOfType<Inventory>(); // Find spillerens inventory i scenen
        if (playerInventory == null)
        {
            Debug.LogError("Player inventory not found in the scene!");
            return null;
        }

        return new CollectItemObjective(itemPrefab, requiredAmount, playerInventory);
    }
}

//using UnityEngine;

//[CreateAssetMenu(menuName = "Objectives/Collect Item Objective")]
//public class CollectItemObjectiveData : ObjectiveData
//{
//    public int amount; // Already exists
//    public Item itemPrefab; // Already exists

//    // Add this property to match the expected 'requiredAmount'
//    public int requiredAmount => amount;

//    public override Objective CreateRuntimeObjective()
//    {
//        return new CollectItemObjective(itemPrefab, requiredAmount, null);
//    }
//}
