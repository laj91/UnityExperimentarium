using UnityEngine;
using System.Collections.Generic;
using System; // For Action
// using UnityEditor.UIElements; // Bruges normalt ikke i runtime scripts

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 20; // Sæt din ønskede størrelse
    public List<ItemStack> items = new List<ItemStack>();

    [Header("Hotbar Settings")]
    public int hotbarSize = 10; // Sæt din ønskede størrelse
    public List<ItemStack> hotbarItems = new List<ItemStack>();

    [Header("References")]
    public InventoryUI inventoryUI; // Træk InventoryUI GameObject her i inspectoren

    public event Action OnInventoryChanged;
    public event Action OnHotbarChanged;
    public List<ItemStack> itemStacks { get; private set; } // Tilføjet for at gemme alle item stacks i inventory og hotbar
    // --- Initialisering (Bedst i Awake) ---
    void Awake()
    {
        // Initialiser begge lister med tomme pladser op til deres størrelse
        InitializeList(items, inventorySize);
        InitializeList(hotbarItems, hotbarSize);

        // Prøv at finde UI hvis den ikke er sat i inspectoren
        if (inventoryUI == null)
        {
            inventoryUI = GetComponent<InventoryUI>(); // Antager UI script er på samme objekt
        }
        if (inventoryUI == null) Debug.LogError("Inventory script mangler reference til InventoryUI!", this);
    }

    void Start()
    {
        // Kald UI update én gang når spillet starter efter alt er initialiseret
        inventoryUI?.UpdateAllUI();
        Debug.Log($"Inventory initialiseret med {items.Count} pladser. Hotbar med {hotbarItems.Count} pladser.");
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
    public List<ItemStack> GetInventory()
    {
        return itemStacks;
    }
    public void NotifyHotbarChanged()
    {
        OnHotbarChanged?.Invoke();
    }
    // Hjælpefunktion til at initialisere lister korrekt
    void InitializeList(List<ItemStack> list, int size)
    {
        // Sikrer listen eksisterer (selvom public burde gøre det)
        if (list == null) list = new List<ItemStack>(size);

        // Fylder op med tomme pladser hvis listen er for kort
        while (list.Count < size)
        {
            list.Add(new ItemStack()); // Standard struct er item=null, amount=0
            //itemStacks.Add(new ItemStack()); // Tilføj til itemStacks listen
        }
        // Korter evt. listen hvis den er for lang (f.eks. efter ændring i inspector)
        if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
    }


    // --- NY CENTRAL METODE TIL AT TILFØJE ITEMS ---
    public bool TryAddItemToPlayer(Item itemToAdd, int amountToAdd = 1)
    {
        if (itemToAdd == null || amountToAdd <= 0)
        {
            Debug.LogWarning("Forsøgte at tilføje et ugyldigt item eller antal (null/<=0).");
            return false;
        }

        int amountRemaining = amountToAdd;
        bool itemAddedOrStacked = false; // Flag til at se om UI skal opdateres

        // --- Trin 1 & 2: Forsøg Hotbar (Stacking og Tomme Pladser) ---
        int originalAmountBeforeHotbar = amountRemaining;
        amountRemaining = AddItemToList(hotbarItems, itemToAdd, amountRemaining);
        if (amountRemaining < originalAmountBeforeHotbar) itemAddedOrStacked = true; // Noget skete i hotbar

        // --- Trin 3: Forsøg Hoved-Inventory hvis nødvendigt ---
        if (amountRemaining > 0)
        {
            // Debug.Log($"Hotbar fuld/ingen plads til {itemToAdd.itemName}. Forsøger main inventory...");
            int originalAmountBeforeMain = amountRemaining;
            amountRemaining = AddItemToList(items, itemToAdd, amountRemaining);
            if (amountRemaining < originalAmountBeforeMain) itemAddedOrStacked = true; // Noget skete i main inv
        }

        // --- Opdater UI og Returner Resultat ---
        if (itemAddedOrStacked)
        {
            Debug.Log($"Opdaterer UI efter forsøg på at tilføje {itemToAdd.itemName}.");
            if (inventoryUI != null)
            {
                inventoryUI?.UpdateAllUI();
                Debug.Log("inventory ui er ikke Null!");
            }
            else 
            {
                Debug.Log("inventory ui er Null!");
            }
            
        }

        if (amountRemaining > 0)
        {
            // Kun log hvis vi forsøgte at tilføje noget men ikke kunne
            if (amountToAdd > 0) Debug.LogWarning($"Inventory & Hotbar fuld. Kunne ikke tilføje de sidste {amountRemaining} x {itemToAdd.itemName}.");
            return false; // Kunne ikke tilføje alt
        }

        return true; // Alt blev tilføjet succesfuldt
    }


    // --- Privat Hjælpefunktion: Tilføjer/Stacker til en specifik liste ---
    // Returnerer det antal, der *ikke* kunne tilføjes. 0 betyder succes.
    private int AddItemToList(List<ItemStack> list, Item itemToAdd, int amountToAdd)
    {
        int amountRemaining = amountToAdd;

        // 1. Forsøg Stacking (kun hvis item er stackable)
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item == itemToAdd && list[i].amount < itemToAdd.stackSize)
                {
                    int spaceAvailable = itemToAdd.stackSize - list[i].amount;
                    int transferAmount = Mathf.Min(amountRemaining, spaceAvailable);

                    if (transferAmount > 0)
                    {
                        ItemStack updatedStack = list[i]; // Kopier struct
                        updatedStack.amount += transferAmount;
                        list[i] = updatedStack; // Gem opdateret struct
                        amountRemaining -= transferAmount;
                        // Debug.Log($"Stacked {transferAmount} {itemToAdd.itemName} i slot {i} af listen.");

                        if (amountRemaining <= 0) return 0; // Alt er tilføjet

                        // Normalt fylder man kun på én stack ad gangen per AddItem kald.
                        // Hvis du vil fylde på *flere* stacks i samme kald, fjern 'break'.
                        // break;
                    }
                }
            }
        }

        // 2. Forsøg Tom Plads
        if (amountRemaining > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].item == null) // Fundet tom plads
                {
                    // Placer op til stackSize i denne plads
                    int amountToPlace = Mathf.Min(amountRemaining, itemToAdd.stackSize);
                    list[i] = new ItemStack { item = itemToAdd, amount = amountToPlace };
                    amountRemaining -= amountToPlace;
                    // Debug.Log($"Placed {amountToPlace} {itemToAdd.itemName} i tom slot {i} af listen.");

                    if (amountRemaining <= 0) return 0; // Alt er placeret (måske over flere slots)
                    // Fortsæt til næste tomme slot hvis amountRemaining > 0
                }
            }
        }

        // Returner det antal, der ikke kunne placeres
        return amountRemaining;
    }


    // --- Din oprindelige AddItem findes ikke længere under det navn ---
    // Den er erstattet af TryAddItemToPlayer og helperen AddItemToList


    // --- RemoveItem (Rettet bug + amount parameter) ---
    public void RemoveItem(Item itemToRemove, int amountToRemove = 1)
    {
        if (itemToRemove == null || amountToRemove <= 0) return;

        int amountLeftToRemove = amountToRemove;

        // Tjek Hotbar først? Eller Inventory? Eller begge? Bestem rækkefølge.
        // Her: Tjekker Hotbar først, derefter Inventory.

        // Tjek Hotbar (loop baglæns ved potentiel sletning/nulstilling)
        for (int i = hotbarItems.Count - 1; i >= 0 && amountLeftToRemove > 0; i--)
        {
            if (hotbarItems[i].item == itemToRemove)
            {
                amountLeftToRemove = RemoveAmountFromSlot(hotbarItems, i, amountLeftToRemove);
            }
        }

        // Tjek Inventory hvis stadig noget skal fjernes
        for (int i = items.Count - 1; i >= 0 && amountLeftToRemove > 0; i--)
        {
            if (items[i].item == itemToRemove)
            {
                amountLeftToRemove = RemoveAmountFromSlot(items, i, amountLeftToRemove);
            }
        }

        // Opdater UI hvis noget blev fjernet
        if (amountLeftToRemove < amountToRemove)
        {
            inventoryUI?.UpdateAllUI();
        }

        if (amountLeftToRemove > 0)
        {
            // Debug.LogWarning($"Kunne ikke fjerne de sidste {amountLeftToRemove} x {itemToRemove.itemName}.");
        }
    }

    // Privat helper til RemoveItem
    private int RemoveAmountFromSlot(List<ItemStack> list, int index, int amountToAttemptToRemove)
    {
        int currentAmountInSlot = list[index].amount;
        int canRemove = Mathf.Min(amountToAttemptToRemove, currentAmountInSlot);

        if (canRemove > 0)
        {
            ItemStack updatedStack = list[index];
            updatedStack.amount -= canRemove;
            list[index] = updatedStack;
            Debug.Log($"Fjernede {canRemove} fra slot {index}. Tilbage: {updatedStack.amount}");

            // RETTET BUG: Nulstil slot i stedet for at fjerne elementet
            if (updatedStack.amount <= 0)
            {
                list[index] = new ItemStack { item = null, amount = 0 }; // Nulstil
                // Debug.Log($"Slot {index} tømt.");
            }
            return amountToAttemptToRemove - canRemove; // Returner resterende amount at fjerne
        }
        return amountToAttemptToRemove; // Intet kunne fjernes fra dette slot
    }


    // --- Opdaterede HasItem / GetItemAmount til at tjekke begge lister ---
    public bool HasItem(Item item, bool checkHotbar = true, bool checkInventory = true)
    {
        if (checkHotbar)
        {
            foreach (ItemStack stack in hotbarItems)
            {
                if (stack.item == item && stack.amount > 0) return true;
            }
        }
        if (checkInventory)
        {
            foreach (ItemStack stack in items)
            {
                if (stack.item == item && stack.amount > 0) return true;
            }
        }
        return false;
    }

    public int GetItemAmount(Item item, bool checkHotbar = true, bool checkInventory = true)
    {
        int totalAmount = 0;

        if (checkHotbar)
        {
            Debug.Log($"Checking hotbar for item: {item?.itemName ?? "null"}");
            foreach (ItemStack stack in hotbarItems)
            {
                if (stack.item != null && stack.item.itemID == item.itemID) // Added null check for stack.item
                {
                    Debug.Log($"Found {stack.amount} of {item.itemName} in hotbar.");
                    totalAmount += stack.amount;
                }
            }
        }

        if (checkInventory)
        {
            Debug.Log($"Checking inventory for item: {item?.itemName ?? "null"}");
            foreach (ItemStack stack in items)
            {
                if (stack.item != null && stack.item.itemID == item.itemID) // Added null check for stack.item
                {
                    Debug.Log($"Found {stack.amount} of {item.itemName} in inventory.");
                    totalAmount += stack.amount;
                }
            }
        }

        Debug.Log($"Total amount of {item?.itemName ?? "null"}: {totalAmount}");
        return totalAmount;
    }


}



// --- ItemStack struct (forbliver den samme) ---
[System.Serializable]
public struct ItemStack
{
    public Item item;
    public int amount;
}

