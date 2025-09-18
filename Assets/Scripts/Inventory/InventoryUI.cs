using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // Brug TextMeshPro
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Events; // Tilføjet for LINQ (bruges i FindDropTargetSlot)

// Sikrer at de nødvendige komponenter er til stede
[RequireComponent(typeof(Inventory), typeof(Hotbar))]
public class InventoryUI : MonoBehaviour
{
    #region Inspector Felter (SerializeField)
    [Header("UI Paneler")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] Hotbar hotbar;

    [Header("Slot Forældre")]
    [SerializeField] private Transform inventorySlotsParent;
    [SerializeField] private Transform hotbarSlotsParent; // Bør sættes i inspektøren!

    [Header("Prefabs")]
    //[SerializeField] private GameObject inventorySlotPrefab; // Bruges ikke i nuværende kode, men god at have
    //[SerializeField] private GameObject hotbarSlotPrefab;   // Bruges ikke i nuværende kode, men god at have
    [SerializeField] private GameObject draggedItemIconPrefab;
    [SerializeField] private MissionManager missionManager; // Reference til MissionManager

    [Header("Held Item")]
    [SerializeField] private float holdDistance = 1.5f; // Afstand fra kameraet
    [SerializeField] private float smoothTime = 0.1f;  // Glathed for bevægelse
   

    #endregion

    #region Private Medlemmer
    // Kernekomponenter
    private Inventory inventory;
    //private Hotbar hotbar;

    // UI Element Lister
    private List<Image> inventorySlotIcons = new List<Image>();
    private List<TextMeshProUGUI> inventorySlotCounts = new List<TextMeshProUGUI>();
    private List<Image> hotbarSlotIcons = new List<Image>();
    private List<TextMeshProUGUI> hotbarSlotCounts = new List<TextMeshProUGUI>();

    // Drag & Drop State
    private bool isDragging = false;
    private bool isDraggingFromHotbar = false;
    private ItemStack draggedItemStack;
    private int dragSourceSlotIndex = -1;
    private GameObject currentDraggedItemIcon;

    // Held Item State
    private GameObject currentlyHeldItemObject;
    private int selectedHotbarSlot = -1;
    private Vector3 currentVelocity = Vector3.zero;
    // private Quaternion currentRotationVelocity = Quaternion.identity; // Ikke brugt i Slerp

    // Event System Caching
    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private Camera mainCamera;

    // Konstante navne (forbedrer robusthed ift. tastefejl)
    private const string ITEM_ICON_NAME = "ItemIcon";
    private const string ITEM_COUNT_NAME = "ItemCount";
    [SerializeField] private InventoryConfig config;
    #endregion

    #region Unity Metoder (Start, Update)

    void Awake()
    {
        if (!InitializeCoreReferences()) return; // Stop hvis kernekomponenter mangler
        if (!InitializeUISlots(inventorySlotsParent, inventorySlotIcons, inventorySlotCounts, "Inventory")) return;
        if (!InitializeUISlots(hotbarSlotsParent, hotbarSlotIcons, hotbarSlotCounts, "Hotbar")) return;

        ValidateSerializedFields();
        InitializeEventSystem();

        // Sørg for at UI'en er opdateret fra start
        //UpdateAllUI();
        // Start med inventory lukket
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            SetCursorState(false);
        }
        else
        {
            Debug.LogError("Inventory Panel reference mangler i InventoryUI!", this);
        }

        inventory.OnInventoryChanged += UpdateInventoryUI;
        inventory.OnHotbarChanged += UpdateHotbarUI;
        mainCamera = Camera.main;
        //// Start med inventory lukket og mus låst (typisk spil-start)
        //inventoryPanel.SetActive(false);
        //SetCursorState(false); // Lås cursor
    }

    void Update()
    {
        HandleInventoryToggleInput(); // Tjek for 'I' tast

        if (inventoryPanel.activeSelf) // Håndter kun drag&drop hvis inventory er åben
        {
            HandleDragAndDropInput();
        }

        HandleHotbarSelectionInput(); // Tjek for 1-0 taster
        HandleHeldItemActions();     // Tjek for brug/kast af holdt item
        UpdateHeldItemTransform();   // Opdater position/rotation af holdt item
    }
    #endregion

    #region Initialisering (Start Hjælpe-metoder)
    private bool InitializeCoreReferences()
    {
        inventory = GetComponent<Inventory>();
        // hotbar = GetComponent<Hotbar>(); // <-- FJERN DENNE LINJE

        if (inventory == null)
        {
            Debug.LogError("Inventory-komponenten ikke fundet!");
            enabled = false;
            return false;
        }
        // Fjern Hotbar null check
        // if (hotbar == null) { ... } // <-- FJERN DENNE BLOK

        Debug.Log($"InventoryUI fandt Inventory component: {inventory != null}");
        return true;
    }

    private bool InitializeUISlots(Transform slotsParent, List<Image> iconList, List<TextMeshProUGUI> countList, string uiType)
    {
        if (slotsParent == null)
        {
            Debug.LogError($"{uiType} Slots Parent er ikke sat i inspektøren!");
            return false;
        }

        iconList.Clear();
        countList.Clear();

        foreach (Transform slotTransform in slotsParent)
        {
            Image icon = slotTransform.Find(ITEM_ICON_NAME)?.GetComponent<Image>();
            TextMeshProUGUI countText = slotTransform.Find(ITEM_COUNT_NAME)?.GetComponent<TextMeshProUGUI>();

            if (icon == null)
            {
                Debug.LogWarning($"ItemIcon ({ITEM_ICON_NAME}) ikke fundet under slot {slotTransform.name} i {uiType}.");
                // Tilføj null for at bevare korrespondance mellem listerne, hvis nødvendigt,
                // eller spring over og juster logikken i UpdateUI tilsvarende. Her tilføjer vi:
                iconList.Add(null);
            }
            else
            {
                iconList.Add(icon);
            }

            if (countText == null)
            {
                // Det er OK hvis countText ikke findes, ikke alle slots behøver det
                // Debug.Log($"ItemCount ({ITEM_COUNT_NAME}) ikke fundet under slot {slotTransform.name} i {uiType}.");
                countList.Add(null); // Tilføj null for at bevare korrespondance
            }
            else
            {
                countList.Add(countText);
            }

            // Tilføj SlotUI komponent til hver slot for nem identifikation
            // Sørg for at InventorySlotUI og HotbarSlotUI scripts eksisterer
            // og er tilføjet til de respektive slot prefabs/objekter.
            if (uiType == "Inventory")
            {
                InventorySlotUI invSlotUI = slotTransform.gameObject.GetComponent<InventorySlotUI>();
                if (invSlotUI == null) invSlotUI = slotTransform.gameObject.AddComponent<InventorySlotUI>();
                invSlotUI.slotIndex = iconList.Count - 1; // Sæt index baseret på position i listen
            }
            else if (uiType == "Hotbar")
            {
                HotbarSlotUI hotbarSlotUI = slotTransform.gameObject.GetComponent<HotbarSlotUI>();
                if (hotbarSlotUI == null) hotbarSlotUI = slotTransform.gameObject.AddComponent<HotbarSlotUI>();
                hotbarSlotUI.slotIndex = iconList.Count - 1; // Sæt index baseret på position i listen
            }
        }

        Debug.Log($"Initialiserede {iconList.Count(i => i != null)} ikoner og {countList.Count(c => c != null)} tekstfelter for {uiType}.");
        return true;
    }

    private void InitializeEventSystem()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>();
    }
    #endregion

    #region Input Håndtering (Update Hjælpe-metoder)
    private void HandleInventoryToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryVisibility();
        }
    }

    private void HandleDragAndDropInput()
    {
        // Start træk
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            StartDragOperation();
        }

        // Opdater ikon position under træk
        if (isDragging)
        {
            UpdateDraggedIconPosition();
        }

        // Stop træk
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            StopDragOperation();
        }
    }

    private void HandleHotbarSelectionInput()
    {
        // Gå igennem tasterne 1-9 og 0
        for (int i = 0; i < 10; i++)
        {
            // KeyCode for 1 er 49, 2 er 50... 9 er 57, 0 er 48
            KeyCode key = (i == 9) ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + i);
            if (Input.GetKeyDown(key))
            {
                SelectHotbarSlot(i);
                break; // Stop loopet når en tast er fundet
            }
        }
    }

    private void HandleHeldItemActions()
    {
        if (currentlyHeldItemObject != null)
        {
            // Kast item (Højreklik)
            if (Input.GetMouseButtonDown(1))
            {
                ThrowHeldItem();
            }
            // Brug item (Venstreklik) - Kun hvis inventory IKKE er åben (for at undgå konflikt med drag/drop)
            else if (Input.GetMouseButtonDown(0) && !inventoryPanel.activeSelf)
            {
                UseHeldItem();
            }
        }
    }
    #endregion

    #region UI Opdatering
    public void UpdateAllUI()
    {
        if (inventory == null) { Debug.LogError("Inventory reference er null i UpdateAllUI!"); return; }
        UpdateInventoryUI();
        UpdateHotbarUI();
        if (selectedHotbarSlot != -1) { UpdateHeldItem(); }
    }

    public void UpdateInventoryUI()
    {
        if (inventory == null) return;
        // Bruger inventory.items - korrekt
        UpdateSlots(inventory.items, inventorySlotIcons, inventorySlotCounts);
    }


    public void UpdateHotbarUI()
    {
        // UpdateSlots(hotbar.hotbarItems, hotbarSlotIcons, hotbarSlotCounts); // <-- GAMMEL
        UpdateSlots(inventory.hotbarItems, hotbarSlotIcons, hotbarSlotCounts); // <-- NY

        // --- ÆNDRING HER: Tjek inventory.hotbarItems ---
        // Tjek også at index er gyldigt før adgang
        // if (selectedHotbarSlot != -1 && hotbar.hotbarItems[selectedHotbarSlot].item == null && currentlyHeldItemObject != null) // <-- GAMMEL
        if (selectedHotbarSlot != -1 && selectedHotbarSlot < inventory.hotbarItems.Count && inventory.hotbarItems[selectedHotbarSlot].item == null && currentlyHeldItemObject != null) // <-- NY (med index tjek)
        {
            UpdateHeldItem();
        }
    }

    // Generisk metode til at opdatere en liste af slots
    private void UpdateSlots(List<ItemStack> items, List<Image> icons, List<TextMeshProUGUI> counts)
    {
        if (items == null || icons == null || counts == null)
        {
            Debug.LogError("Cannot update UI slots: One of the lists is null.");
            return;
        }

        for (int i = 0; i < icons.Count; i++)
        {
            var itemStack = i < items.Count ? items[i] : new ItemStack();
            UpdateSingleSlotUI(icons[i], i < counts.Count ? counts[i] : null, itemStack);
        }
    }

    // Opdaterer et enkelt slot
    private void UpdateSingleSlotUI(Image icon, TextMeshProUGUI countText, ItemStack itemStack)
    {
        Debug.Log("I UpdateSingleSlotUI");
        if (icon == null) return; // Kan ikke opdatere hvis ikonet mangler

        bool hasItem = itemStack.item != null && itemStack.amount > 0;
        Debug.Log("I UpdateSingleSlotUI, HasItem = " + hasItem);

        icon.enabled = hasItem;
        if (hasItem)
        {
            icon.sprite = itemStack.item.icon;
        }

        if (countText != null)
        {
            bool showCount = hasItem && itemStack.amount > 1 && itemStack.item.isStackable;
            countText.enabled = showCount;
            if (showCount)
            {
                countText.text = itemStack.amount.ToString();
            }
            else
            {
                countText.text = ""; // Ryd teksten hvis den ikke skal vises
            }
        }
    }
    #endregion

    #region Inventory Synlighed og Cursor
    public void ToggleInventoryVisibility()
    {
        bool isNowVisible = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isNowVisible);
        SetCursorState(isNowVisible);

        // Hvis inventory lukkes mens man trækker, afbryd trækket
        if (!isNowVisible && isDragging)
        {
            StopDragOperation(true); // Force stop without drop logic
        }
    }

    private void SetCursorState(bool visibleAndUnlocked)
    {
        Cursor.visible = visibleAndUnlocked;
        Cursor.lockState = visibleAndUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
        // Debug.Log($"Cursor state set: Visible={visibleAndUnlocked}, LockState={Cursor.lockState}");
    }
    private void StartDragOperation()
    {
        Debug.Log("StartDragOperation called."); // Debug: Method entry point

        pointerEventData.position = Input.mousePosition;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        Debug.Log($"Raycast results count: {raycastResults.Count}"); // Debug: Raycast results

        foreach (var hit in raycastResults)
        {
            Debug.Log($"Raycast hit: {hit.gameObject.name}"); // Debug: Raycast hit object

            // Check for InventorySlotUI or HotbarSlotUI instead of ISlotUI
            var inventorySlotUI = hit.gameObject.GetComponent<InventorySlotUI>();
            var hotbarSlotUI = hit.gameObject.GetComponent<HotbarSlotUI>();

            if (inventorySlotUI != null)
            {
                Debug.Log($"InventorySlotUI found: SlotIndex: {inventorySlotUI.slotIndex}"); // Debug: SlotUI details

                if (inventorySlotUI.slotIndex < inventory.items.Count && inventory.items[inventorySlotUI.slotIndex].item != null)
                {
                    Debug.Log($"Valid item found in inventory at index {inventorySlotUI.slotIndex}: {inventory.items[inventorySlotUI.slotIndex].item.itemName}"); // Debug: Valid item

                    StartDraggingItem(inventorySlotUI.slotIndex, inventory.items[inventorySlotUI.slotIndex], false);
                    return;
                }
            }
            else if (hotbarSlotUI != null)
            {
                Debug.Log($"HotbarSlotUI found: SlotIndex: {hotbarSlotUI.slotIndex}"); // Debug: SlotUI details

                if (hotbarSlotUI.slotIndex < inventory.hotbarItems.Count && inventory.hotbarItems[hotbarSlotUI.slotIndex].item != null)
                {
                    Debug.Log($"Valid item found in hotbar at index {hotbarSlotUI.slotIndex}: {inventory.hotbarItems[hotbarSlotUI.slotIndex].item.itemName}"); // Debug: Valid item

                    StartDraggingItem(hotbarSlotUI.slotIndex, inventory.hotbarItems[hotbarSlotUI.slotIndex], true);
                    return;
                }
            }
            else
            {
                Debug.Log($"No InventorySlotUI or HotbarSlotUI component found on {hit.gameObject.name}."); // Debug: No valid slot
            }
        }

        Debug.Log("StartDragOperation completed without starting a drag."); // Debug: Method exit point
    }

    private void StartDraggingItem(int slotIndex, ItemStack itemStack, bool fromHotbar)
    {
        if (itemStack.item == null) return; // Sikkerhedstjek

        isDragging = true;
        isDraggingFromHotbar = fromHotbar;
        draggedItemStack = itemStack;
        dragSourceSlotIndex = slotIndex;

        // Debug.Log($"Startede træk af {draggedItemStack.item.itemName} fra slot {dragSourceSlotIndex} ({(fromHotbar ? "hotbar" : "inventory")})");

        // Skjul item i kilde-slot (midlertidigt)
        UpdateSingleSlotUI(
            fromHotbar ? hotbarSlotIcons[slotIndex] : inventorySlotIcons[slotIndex],
            fromHotbar ? hotbarSlotCounts[slotIndex] : inventorySlotCounts[slotIndex],
            new ItemStack() // Vis som tom
        );

        // Opret og konfigurer det visuelle ikon, der følger musen
        if (draggedItemIconPrefab != null)
        {
            currentDraggedItemIcon = Instantiate(draggedItemIconPrefab, inventoryPanel.transform); // Parent under inventory panelet
            Image draggedIconImage = currentDraggedItemIcon.GetComponent<Image>();
            if (draggedIconImage != null)
            {
                draggedIconImage.sprite = draggedItemStack.item.icon;
                draggedIconImage.raycastTarget = false; // Undgå at ikonet blokerer for raycasts til slots nedenunder
                // draggedIconImage.SetNativeSize(); // Overvej om dette er nødvendigt/ønsket
            }
            UpdateDraggedIconPosition(); // Sæt startposition
        }
    }

    private void UpdateDraggedIconPosition()
    {
        if (currentDraggedItemIcon != null)
        {
            currentDraggedItemIcon.transform.position = Input.mousePosition;
        }
    }

    private void StopDragOperation(bool cancelOperation = false)
    {
        if (inventory == null) return; // Safety check
        if (currentDraggedItemIcon != null) { Destroy(currentDraggedItemIcon); currentDraggedItemIcon = null; }

        if (!cancelOperation)
        {
            GameObject dropTargetObject = FindDropTargetSlot();
            if (dropTargetObject != null)
            {
                InventorySlotUI invSlotTarget = dropTargetObject.GetComponent<InventorySlotUI>();
                if (invSlotTarget != null)
                {
                    // Bestem sourceList korrekt
                    List<ItemStack> sourceList = isDraggingFromHotbar ? inventory.hotbarItems : inventory.items; // <-- NY: Brug inventory.hotbarItems
                    HandleDropOnSlot(inventory.items, invSlotTarget.slotIndex, sourceList, dragSourceSlotIndex, draggedItemStack);
                }
                else
                {
                    HotbarSlotUI hotbarSlotTarget = dropTargetObject.GetComponent<HotbarSlotUI>();
                    if (hotbarSlotTarget != null)
                    {
                        // Bestem sourceList korrekt
                        List<ItemStack> sourceList = isDraggingFromHotbar ? inventory.hotbarItems : inventory.items; // <-- NY: Brug inventory.hotbarItems
                        HandleDropOnSlot(inventory.hotbarItems, hotbarSlotTarget.slotIndex, sourceList, dragSourceSlotIndex, draggedItemStack); // <-- NY: Target er inventory.hotbarItems
                    }
                }
            }
            else
            {
                HandleDropOutsideUI();
            }
        }
        // else { /* Cancelled */ }

        ResetDragState();
        UpdateAllUI();
    }

    // Finder det øverste UI element under musen, der er et slot
    private GameObject FindDropTargetSlot()
    {
        pointerEventData.position = Input.mousePosition;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // Find det første resultat, der har enten InventorySlotUI eller HotbarSlotUI
        RaycastResult hit = raycastResults.FirstOrDefault(r => r.gameObject.GetComponent<InventorySlotUI>() != null || r.gameObject.GetComponent<HotbarSlotUI>() != null);

        if (hit.isValid)
        {
            // Debug.Log($"Dropped på: {hit.gameObject.name}");
            return hit.gameObject;
        }

        // Debug.Log("Dropped uden for et gyldigt slot.");
        return null;
    }

    // Generisk metode til at håndtere drop på et slot (enten inventory eller hotbar)
    private void HandleDropOnSlot(List<ItemStack> targetList, int targetIndex, List<ItemStack> sourceList, int dragSourceSlotIndex, ItemStack droppedStack)
    {
        // Debug.Log($"HandleDropOnSlot: TargetList={targetList.Count}, TargetIndex={targetIndex}, SourceList={sourceList.Count}, dragSourceSlotIndex={dragSourceSlotIndex}");

        if (targetIndex < 0 || targetIndex >= targetList.Count)
        {
            Debug.LogWarning("Ugyldigt drop target index: " + targetIndex);

            return;
        }

        ItemStack targetSlotStack = targetList[targetIndex];

        // Case 1: Drop på et tomt slot
        if (targetSlotStack.item == null)
        {
            targetList[targetIndex] = droppedStack; // Placer hele stacken
            sourceList[dragSourceSlotIndex] = new ItemStack(); // Tøm kilde slot
                                                               // Debug.Log($"Flyttede {droppedStack.item.itemName} til tomt slot {targetIndex}");
        }
        // Case 2: Drop på et slot med samme item type (forsøg at stacke)
        else if (targetSlotStack.item == droppedStack.item && targetSlotStack.item.isStackable)
        {
            int spaceAvailable = targetSlotStack.item.stackSize - targetSlotStack.amount;
            int amountToTransfer = Mathf.Min(droppedStack.amount, spaceAvailable);

            if (amountToTransfer > 0)
            {
                // Opdater target stack (husk at ItemStack er en struct!)
                ItemStack updatedTargetStack = targetSlotStack;
                updatedTargetStack.amount += amountToTransfer;
                targetList[targetIndex] = updatedTargetStack;

                // Opdater source stack
                ItemStack updatedSourceStack = droppedStack;
                updatedSourceStack.amount -= amountToTransfer;

                // Hvis source stack er tom, tøm den
                if (updatedSourceStack.amount <= 0)
                {
                    sourceList[dragSourceSlotIndex] = new ItemStack();
                }
                else
                {
                    sourceList[dragSourceSlotIndex] = updatedSourceStack; // Læg resten tilbage
                }
                // Debug.Log($"Stacked {amountToTransfer} {droppedStack.item.itemName} på slot {targetIndex}");
            }
            else
            {
                // Ingen plads til at stacke - byt plads hvis muligt
                SwapItems(targetList, targetIndex, sourceList, dragSourceSlotIndex);
            }
        }
        // Case 3: Drop på et slot med et andet item (byt plads)
        else
        {
            SwapItems(targetList, targetIndex, sourceList, dragSourceSlotIndex);
        }
    }

    private void SwapItems(List<ItemStack> listA, int indexA, List<ItemStack> listB, int indexB)
    {
        // Debug.Log($"Bytter items mellem ({GetListName(listA)}[{indexA}]) og ({GetListName(listB)}[{indexB}])");
        ItemStack temp = listA[indexA];
        listA[indexA] = listB[indexB];
        listB[indexB] = temp;
    }

    // Hjælpefunktion til debugging
    // GetListName - Opdateret hjælper
    private string GetListName(List<ItemStack> list)
    {
        if (inventory == null) return "Ukendt (Inventory null)";
        if (list == inventory.items) return "Inventory";
        // --- ÆNDRING HER: Brug inventory.hotbarItems ---
        if (list == inventory.hotbarItems) return "Hotbar"; // <-- NY
        return "Ukendt Liste";
    }

    private void HandleDropOutsideUI()
    {
        // Log at der blev droppet udenfor
        Debug.Log($"Droppede {draggedItemStack.item?.itemName ?? "UNKNOWN"} udenfor UI. Returnerer til kilde.");

    }



    private void ResetDragState()
    {
        isDragging = false;
        isDraggingFromHotbar = false;
        draggedItemStack = new ItemStack(); // Ryd struct
        dragSourceSlotIndex = -1;
        if (currentDraggedItemIcon != null)
        {
            Destroy(currentDraggedItemIcon);
            currentDraggedItemIcon = null;
        }
    }
    #endregion

    #region Hotbar Valg og Held Item Logik

    void SelectHotbarSlot(int slotIndex)
    {
        if (inventory == null) return;
        // --- ÆNDRING HER: Brug inventory.hotbarItems ---
        // if (slotIndex < 0 || slotIndex >= hotbar.hotbarItems.Count) return; // <-- GAMMEL
        if (slotIndex < 0 || slotIndex >= inventory.hotbarItems.Count) return; // <-- NY

        selectedHotbarSlot = slotIndex;
        UpdateHeldItem();
    }


    void UpdateHeldItem()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory er null i UpdateHeldItem!");
            return;
        }

        if (currentlyHeldItemObject != null)
        {
            Destroy(currentlyHeldItemObject);
            currentlyHeldItemObject = null;
        }

        if (selectedHotbarSlot == -1 || selectedHotbarSlot >= inventory.hotbarItems.Count)
        {
            Debug.LogWarning("Ingen gyldig hotbar slot valgt eller slot er uden for rækkevidde.");
            return;
        }

        Item selectedItem = inventory.hotbarItems[selectedHotbarSlot].item;

        if (selectedItem == null)
        {
            Debug.LogWarning($"Ingen item fundet i hotbar slot {selectedHotbarSlot}.");
            return;
        }

        Debug.Log($"Valgt item: {selectedItem.itemName}, isWeapon: {selectedItem.isWeapon}");
        WeaponManager weaponManager = FindFirstObjectByType<WeaponManager>();
        if (selectedItem.isWeapon)
        {
            Debug.Log("selectedItem er et våben!");

            // Brug WeaponManager til at aktivere våbenet
            
            if (weaponManager != null)
            {
                weaponManager.ActivateWeapon(true, selectedItem.itemName);
            }
            else
            {
                Debug.LogError("WeaponManager ikke fundet i scenen!");
            }

            return; // Stop yderligere behandling
        }

        if (selectedItem.prefab != null)
        {
            Debug.Log("selectedItem er ikke et våben!");
            weaponManager.ActivateWeapon(false); // Deaktiver våbenet
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * holdDistance;
            Quaternion spawnRotation = Camera.main.transform.rotation;
            currentlyHeldItemObject = Instantiate(selectedItem.prefab, spawnPosition, spawnRotation);
            Rigidbody rb = currentlyHeldItemObject.GetComponent<Rigidbody>() ?? currentlyHeldItemObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;

            if (currentlyHeldItemObject.GetComponent<Collider>() == null)
            {
                currentlyHeldItemObject.AddComponent<BoxCollider>();
            }
        }
        else
        {
            Debug.LogError($"Item '{selectedItem.itemName}' har ikke et prefab tilknyttet!");
        }
    }


    // Kaldes fra Update for at glatte bevægelsen af det holdte item
    private void UpdateHeldItemTransform()
    {
        if (currentlyHeldItemObject == null) return;

        Vector3 targetPosition = Camera.main.transform.position + Camera.main.transform.forward * config.holdDistance;
        Quaternion targetRotation = Camera.main.transform.rotation;

        currentlyHeldItemObject.transform.position = Vector3.SmoothDamp(
            currentlyHeldItemObject.transform.position, targetPosition, ref currentVelocity, config.smoothTime
        );
        currentlyHeldItemObject.transform.rotation = Quaternion.Slerp(
            currentlyHeldItemObject.transform.rotation, targetRotation, Time.deltaTime / (config.smoothTime * 0.5f)
        );
    }
 

    void ThrowHeldItem()
    {
        if (inventory == null) return;
        if (currentlyHeldItemObject == null || selectedHotbarSlot == -1) return;

        // --- ÆNDRING HER: Brug inventory.hotbarItems ---
        if (selectedHotbarSlot >= inventory.hotbarItems.Count) return; // Ekstra sikkerhedstjek

        Rigidbody rb = currentlyHeldItemObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Gør klar til fysik
            rb.useGravity = true;
            rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.Impulse);
        }

        // Fjern fra hotbar data (inventory.hotbarItems)
        ItemStack currentStack = inventory.hotbarItems[selectedHotbarSlot]; // <-- NY
        currentStack.amount--;
        if (currentStack.amount <= 0)
        {
            Debug.Log($"Kastede {currentStack.item.itemName}, med ID {currentStack.item.itemID} fra slot {selectedHotbarSlot}");
            inventory.hotbarItems[selectedHotbarSlot] = new ItemStack();
            selectedHotbarSlot = -1;

            // Opdater mission
            missionManager.OnInventoryChanged(); // <-- NYT
        }
        else
        {
            inventory.hotbarItems[selectedHotbarSlot] = currentStack;
            missionManager.OnInventoryChanged(); // <-- også her
        }


        currentlyHeldItemObject = null;
        UpdateHotbarUI();
        UpdateHeldItem();
    }

    void UseHeldItem()
    {
        if (inventory == null) return;
        if (currentlyHeldItemObject == null || selectedHotbarSlot == -1) return;

        // --- ÆNDRING HER: Brug inventory.hotbarItems ---
        if (selectedHotbarSlot >= inventory.hotbarItems.Count) return; // Ekstra sikkerhedstjek
        ItemStack stackToUse = inventory.hotbarItems[selectedHotbarSlot]; // <-- NY
        if (stackToUse.item == null) return;

        Debug.Log($"Forsøger at bruge {stackToUse.item.itemName} fra slot {selectedHotbarSlot}");
        // ... (Brugslogik placeholder) ...

        bool consumed = false; // Sæt til true hvis item forbruges
        if (consumed)
        {
            ItemStack currentStack = inventory.hotbarItems[selectedHotbarSlot]; // <-- NY
            currentStack.amount--;
            if (currentStack.amount <= 0)
            {
                inventory.hotbarItems[selectedHotbarSlot] = new ItemStack(); // <-- NY
                selectedHotbarSlot = -1;
                Destroy(currentlyHeldItemObject);
                currentlyHeldItemObject = null;
            }
            else
            {
                inventory.hotbarItems[selectedHotbarSlot] = currentStack; // <-- NY
            }
            UpdateHotbarUI();
            UpdateHeldItem();
        }
    }
    private void Log(string message, Object context = null)
    {
        if (Application.isEditor)
        {
            Debug.Log(message, context);
        }
    }
    private void ValidateSerializedFields()
    {
        if (inventoryPanel == null) Debug.LogError("Inventory Panel is not assigned!", this);
        if (hotbarPanel == null) Debug.LogError("Hotbar Panel is not assigned!", this);
        if (inventorySlotsParent == null) Debug.LogError("Inventory Slots Parent is not assigned!", this);
        if (hotbarSlotsParent == null) Debug.LogError("Hotbar Slots Parent is not assigned!", this);
    }

    
}

    #endregion
    