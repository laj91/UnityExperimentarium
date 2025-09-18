using UnityEngine;
using System.Collections.Generic;
using System;

public class Hotbar : MonoBehaviour
{
    public int hotbarSize = 9;
    public List<ItemStack> hotbarItems = new List<ItemStack>();

    // Reference til Inventory-scriptet p� spilleren
    private Inventory inventory;
    // Holder styr p� den nuv�rende aktive hotbar slot
    public int currentHotbarIndex = -1; // -1 betyder ingen er valgt
    void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory-komponenten ikke fundet p� dette GameObject!");
            enabled = false;
        }

        // Sikrer at hotbarItems listen har den korrekte st�rrelse
        while (hotbarItems.Count < hotbarSize)
        {
            hotbarItems.Add(new ItemStack { item = null, amount = 0 });
        }
        // V�lg den f�rste genstand i hotbar'en ved start, hvis der er nogen
        SelectFirstAvailableItem();
    }

  
    // Finder og v�lger den f�rste genstand i hotbar'en (med lavest indeks)
    private void SelectFirstAvailableItem()
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            if (hotbarItems[i].item != null)
            {
                currentHotbarIndex = i;
                Debug.Log($"Automatisk valgt genstand p� slot {currentHotbarIndex}");
                // Her skal du opdatere UI'en til at vise den valgte slot
                return;
            }
        }
        // Hvis ingen genstande er fundet, s�t currentHotbarIndex til -1
        currentHotbarIndex = -1;
        // Opdater UI'en til at vise ingen valgt
    }
    // Fors�ger at placere en genstand fra inventory i hotbar'en
    public bool PlaceItemInHotbar(Item item, int inventoryIndex, int hotbarIndex)
    {
        if (hotbarIndex >= 0 && hotbarIndex < hotbarSize && inventoryIndex >= 0 && inventoryIndex < inventory.items.Count)
        {
            if (hotbarItems[hotbarIndex].item == null)
            {
                hotbarItems[hotbarIndex] = inventory.items[inventoryIndex];
                Debug.Log($"Placerede {item.itemName} i hotbar slot {hotbarIndex}");
                // Her skal vi ogs� opdatere UI'en for denne slot

                // Hvis hotbar'en var tom f�r, eller hvis den placerede genstand er p� indeks 0, skal vi m�ske v�lge den
                if (currentHotbarIndex == -1 || hotbarIndex == 0)
                {
                    SelectFirstAvailableItem();
                }
                else if (currentHotbarIndex != -1 && hotbarItems[currentHotbarIndex].item == null)
                {
                    SelectFirstAvailableItem();
                }

                return true;
            }
            else
            {
                Debug.Log($"Hotbar slot {hotbarIndex} er allerede optaget.");
                return false;
            }
        }
        else
        {
            Debug.Log("Ugyldigt hotbar eller inventory indeks.");
            return false;
        }
    }
    


    // Fjerner en genstand fra hotbar'en
    public void RemoveItemFromHotbar(int hotbarIndex)
    {
        if (hotbarIndex >= 0 && hotbarIndex < hotbarSize && hotbarItems[hotbarIndex].item != null)
        {
            Debug.Log($"Fjernede {hotbarItems[hotbarIndex].item.itemName} fra hotbar slot {hotbarIndex}");
            hotbarItems[hotbarIndex] = new ItemStack { item = null, amount = 0 };
            // Her skal vi ogs� opdatere UI'en for denne slot

            // Hvis den fjernede genstand var den aktive, skal vi v�lge den n�ste tilg�ngelige
            if (hotbarIndex == currentHotbarIndex)
            {
                SelectFirstAvailableItem();
            }
        }
        else
        {
            Debug.Log("Ugyldigt eller tomt hotbar indeks.");
        }


    }
    

    // Henter genstanden i en given hotbar slot
    public Item GetItemInHotbar(int hotbarIndex)
    {
        if (hotbarIndex >= 0 && hotbarIndex < hotbarSize)
        {
            return hotbarItems[hotbarIndex].item;
        }
        return null;
    }

    // Henter den genstand, der er i h�nden (den med det laveste indeks)
    public Item GetCurrentItemInHand()
    {
        if (currentHotbarIndex != -1 && currentHotbarIndex < hotbarSize)
        {
            return hotbarItems[currentHotbarIndex].item;
        }
        return null;
    }

    // Aktiverer genstanden i den nuv�rende aktive hotbar slot
    public void ActivateCurrentHotbarItem()
    {
        if (currentHotbarIndex != -1)
        {
            ActivateHotbarItem(currentHotbarIndex);
        }
        else
        {
            Debug.Log("Ingen genstand valgt i hotbar'en.");
        }
    }

    // Aktiverer genstanden i en given hotbar slot(kommer vi til senere)
    public void ActivateHotbarItem(int hotbarIndex)
    {
        Item itemToActivate = GetItemInHotbar(hotbarIndex);
        if (itemToActivate != null)
        {
            Debug.Log($"Aktiverede {itemToActivate.itemName} fra hotbar slot {hotbarIndex}");
            // Implementer aktiveringslogik her (f.eks. brug af v�ben, drikke en potion osv.)
        }
        else
        {
            Debug.Log($"Intet at aktivere i hotbar slot {hotbarIndex}.");
        }
    }
    
}