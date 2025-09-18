using UnityEngine;
using System.Collections.Generic;
using System; // For Action
// using UnityEditor.UIElements; // Bruges normalt ikke i runtime scripts

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 20; // S�t din �nskede st�rrelse
    public List<ItemStack> items = new List<ItemStack>();

    [Header("Hotbar Settings")]
    public int hotbarSize = 10; // S�t din �nskede st�rrelse
    public List<ItemStack> hotbarItems = new List<ItemStack>();

    [Header("References")]
    public InventoryUI inventoryUI; // Tr�k InventoryUI GameObject her i inspectoren

    public event Action OnInventoryChanged;
    public event Action OnHotbarChanged;
    public List<ItemStack> itemStacks { get; private set; } // Tilf�jet for at gemme alle item stacks i inventory og hotbar
    // --- Initialisering (Bedst i Awake) ---
    void Awake()
    {
        // Initialiser begge lister med tomme pladser op til deres st�rrelse
        InitializeList(items, inventorySize);
        InitializeList(hotbarItems, hotbarSize);

        // Pr�v at finde UI hvis den ikke er sat i inspectoren
        if (inventoryUI == null)
        {
            inventoryUI = GetComponent<InventoryUI>(); // Antager UI script er p� samme objekt
        }
        if (inventoryUI == null) Debug.LogError("Inventory script mangler reference til InventoryUI!", this);
    }

    void Start()
    {
        // Kald UI update �n gang n�r spillet starter efter alt er initialiseret
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
    // Hj�lpefunktion til at initialisere lister korrekt
    void InitializeList(List<ItemStack> list, int size)
    {
        // Sikrer listen eksisterer (selvom public burde g�re det)
        if (list == null) list = new List<ItemStack>(size);

        // Fylder op med tomme pladser hvis listen er for kort
        while (list.Count < size)
        {
            list.Add(new ItemStack()); // Standard struct er item=null, amount=0
            //itemStacks.Add(new ItemStack()); // Tilf�j til itemStacks listen
        }
        // Korter evt. listen hvis den er for lang (f.eks. efter �ndring i inspector)
        if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
    }


    // --- NY CENTRAL METODE TIL AT TILF�JE ITEMS ---
    public bool TryAddItemToPlayer(Item itemToAdd, int amountToAdd = 1)
    {
        if (itemToAdd == null || amountToAdd <= 0)
        {
            Debug.LogWarning("Fors�gte at tilf�je et ugyldigt item eller antal (null/<=0).");
            return false;
        }

        int amountRemaining = amountToAdd;
        bool itemAddedOrStacked = false; // Flag til at se om UI skal opdateres

        // --- Trin 1 & 2: Fors�g Hotbar (Stacking og Tomme Pladser) ---
        int originalAmountBeforeHotbar = amountRemaining;
        amountRemaining = AddItemToList(hotbarItems, itemToAdd, amountRemaining);
        if (amountRemaining < originalAmountBeforeHotbar) itemAddedOrStacked = true; // Noget skete i hotbar

        // --- Trin 3: Fors�g Hoved-Inventory hvis n�dvendigt ---
        if (amountRemaining > 0)
        {
            // Debug.Log($"Hotbar fuld/ingen plads til {itemToAdd.itemName}. Fors�ger main inventory...");
            int originalAmountBeforeMain = amountRemaining;
            amountRemaining = AddItemToList(items, itemToAdd, amountRemaining);
            if (amountRemaining < originalAmountBeforeMain) itemAddedOrStacked = true; // Noget skete i main inv
        }

        // --- Opdater UI og Returner Resultat ---
        if (itemAddedOrStacked)
        {
            Debug.Log($"Opdaterer UI efter fors�g p� at tilf�je {itemToAdd.itemName}.");
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
            // Kun log hvis vi fors�gte at tilf�je noget men ikke kunne
            if (amountToAdd > 0) Debug.LogWarning($"Inventory & Hotbar fuld. Kunne ikke tilf�je de sidste {amountRemaining} x {itemToAdd.itemName}.");
            return false; // Kunne ikke tilf�je alt
        }

        return true; // Alt blev tilf�jet succesfuldt
    }


    // --- Privat Hj�lpefunktion: Tilf�jer/Stacker til en specifik liste ---
    // Returnerer det antal, der *ikke* kunne tilf�jes. 0 betyder succes.
    private int AddItemToList(List<ItemStack> list, Item itemToAdd, int amountToAdd)
    {
        int amountRemaining = amountToAdd;

        // 1. Fors�g Stacking (kun hvis item er stackable)
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

                        if (amountRemaining <= 0) return 0; // Alt er tilf�jet

                        // Normalt fylder man kun p� �n stack ad gangen per AddItem kald.
                        // Hvis du vil fylde p� *flere* stacks i samme kald, fjern 'break'.
                        // break;
                    }
                }
            }
        }

        // 2. Fors�g Tom Plads
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

                    if (amountRemaining <= 0) return 0; // Alt er placeret (m�ske over flere slots)
                    // Forts�t til n�ste tomme slot hvis amountRemaining > 0
                }
            }
        }

        // Returner det antal, der ikke kunne placeres
        return amountRemaining;
    }


    // --- Din oprindelige AddItem findes ikke l�ngere under det navn ---
    // Den er erstattet af TryAddItemToPlayer og helperen AddItemToList


    // --- RemoveItem (Rettet bug + amount parameter) ---
    public void RemoveItem(Item itemToRemove, int amountToRemove = 1)
    {
        if (itemToRemove == null || amountToRemove <= 0) return;

        int amountLeftToRemove = amountToRemove;

        // Tjek Hotbar f�rst? Eller Inventory? Eller begge? Bestem r�kkef�lge.
        // Her: Tjekker Hotbar f�rst, derefter Inventory.

        // Tjek Hotbar (loop bagl�ns ved potentiel sletning/nulstilling)
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
                // Debug.Log($"Slot {index} t�mt.");
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

