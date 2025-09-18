using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 3f;
    public Camera playerCamera; // Offentlig variabel til at tr�kke kameraet ind i
    private MissionManager missionManager;
    private Inventory playerInventory;
    private InventoryUI inventoryUI; // Tilf�jet reference til InventoryUI


    void Start()
    {

        missionManager = FindAnyObjectByType<MissionManager>();
        if (missionManager == null)
        {
            Debug.LogError("MissionManager ikke fundet i scenen!");
        }

        playerInventory = GetComponent<Inventory>();
        inventoryUI = GetComponent<InventoryUI>(); // Hent InventoryUI-komponenten
        if (playerInventory == null)
        {
            Debug.LogError("Inventory-komponenten ikke fundet p� spilleren.");
            enabled = false;
        }
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI-komponenten ikke fundet p� spilleren.");
            enabled = false;
        }
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera er ikke sat i PlayerInteraction scriptet!");
            enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInventory != null && playerCamera != null && inventoryUI != null)
        {
            // Find alle Pickup-objekter inden for r�kkevidde og synsfelt
            Pickup[] nearbyPickups = FindObjectsOfType<Pickup>()
                .Where(p => p.DistanceToPlayer() <= interactionRange && p.IsInView(playerCamera))
                .ToArray();

            if (nearbyPickups.Length > 0)
            {
                // Find den n�rmeste Pickup
                Pickup closestPickup = nearbyPickups
                    .OrderBy(p => p.DistanceToPlayer())
                    .First();

                // ------ �NDRING STARTER HER ------

                // S�t antallet der skal samles op. Default er 1.
                // Hvis dit Pickup script har et antal (f.eks. closestPickup.itemAmount), brug det her:
                int amountToCollect = 1; // Standard antal
                if (closestPickup != null && closestPickup.item != null)
                {
                    // Kald den nye metode p� Inventory scriptet
                    if (playerInventory.TryAddItemToPlayer(closestPickup.item, amountToCollect))
                    {
                        Debug.Log($"Samlede {amountToCollect} x {closestPickup.item.itemName} op.");

                        if (missionManager != null)
                        {
                            missionManager.OnInventoryChanged(); // <-- Brug kun denne
                        }

                        Destroy(closestPickup.gameObject);
                    }
                    else
                    {
                        // Fejl - B�de Hotbar og Inventory var fulde.
                        Debug.Log("Inventory og Hotbar er fuld! Kunne ikke samle op."); // Opdateret besked
                    }
                    // ------ �NDRING SLUTTER HER ------
                }


            }
            else
            {
                Debug.Log("Ingen genstande at samle op i n�rheden og inden for synsfeltet.");
            }
        }
    }
}