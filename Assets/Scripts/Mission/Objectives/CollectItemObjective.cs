using System;
using UnityEngine;

public class CollectItemObjective : Objective
{
    private Item targetItem;
    private int requiredAmount;
    private Inventory playerInventory;

    public int RequiredAmount => requiredAmount;
    public int CollectedAmount => CountItemInInventory(targetItem.itemID);

    public string ItemName => targetItem.itemName; // Tilføjet metode til at hente itemets navn


    public CollectItemObjective(Item item, int amount, Inventory inventory)
    {
        targetItem = item;
        requiredAmount = amount;
        playerInventory = inventory;
    }
    public override string GetObjectiveDescription()
    {
        return $"Collect {targetItem.itemName}: {CountItemInInventory(targetItem.itemID)}/{requiredAmount}";
    }
    public void ItemCollected(int collectedItemID, Action<int> onItemDestroyed)
    {
        Debug.Log($"ItemCollected called with collectedItemID: {collectedItemID}");

        if (collectedItemID == targetItem.itemID)
        {
            Debug.Log($"Collected item matches target item: {targetItem.itemName} (ID: {targetItem.itemID})");
            Debug.Log($"Collected amount is now: {CollectedAmount}/{requiredAmount}");
       

            if (CollectedAmount >= requiredAmount)
            {
                Debug.Log($"Collected amount reached or exceeded required amount: {CollectedAmount}/{requiredAmount}");
                CompleteObjective();
            }

            onItemDestroyed?.Invoke(collectedItemID);
        }
    }

    public void DiscardItem(int discardedItemID, Action<int> onItemDestroyed)
    {
        Debug.Log($"DiscardItem called with discardedItemID: {discardedItemID}");
        if (discardedItemID == targetItem.itemID)
        {
            Debug.Log($"Discarded item matches target item: {targetItem.itemName} (ID: {targetItem.itemID})");
            onItemDestroyed?.Invoke(discardedItemID);
            // Husk at kalde CheckProgress() bagefter udenfor denne metode
        }
    }

    public void CheckProgress(Inventory inventory)
    {
        Debug.Log("CheckProgress called.");
        playerInventory = inventory;
        Debug.Log($"Checked inventory. Current collected amount: {CollectedAmount}/{requiredAmount}");

        if (CollectedAmount >= requiredAmount)
        {
            Debug.Log("Required amount collected. Completing objective.");
            CompleteObjective();
        }
        else
        {
            Debug.Log("Required amount not yet collected. Objective not completed.");
            isCompleted = false;
        }
    }

    private int CountItemInInventory(int itemID)
    {
        Debug.Log($"CountItemInInventory called with itemID: {itemID}");

        var tempItem = ScriptableObject.CreateInstance<Item>();
        tempItem.itemID = itemID;

        return playerInventory.GetItemAmount(tempItem);
    }

    public override void CompleteObjective()
    {
        isCompleted = true;
        Debug.Log($"Objective completed: Collect {requiredAmount} of {targetItem.itemName}");
    }
}
