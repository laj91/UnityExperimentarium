using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item;
    public float interactionRange = 3f;
    public float pickupRadius = 0.1f; // Radius omkring midten af skærmen (viewport-koordinater)
    private Transform playerTransform;
    private Inventory playerInventory; // Tilføjet reference til Inventory
    private PlayerInteraction playerInteraction; // Tilføjet reference til PlayerInteraction
    private int itemID; // ID for at identificere itemet

    void Start()
    {
        itemID = item.itemID; // Hent itemID fra itemet
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("player name: " +  player.name);
            playerInventory = player.GetComponentInParent<Inventory>(); // Hent Inventory-komponenten
            playerInteraction = player.GetComponentInParent<PlayerInteraction>(); // Hent PlayerInteraction-komponenten
            if (playerInventory == null)
            {
                Debug.LogError("Inventory-komponenten ikke fundet på spilleren.");
                enabled = false;
            }
            if (playerInteraction == null)
            {
                Debug.LogError("PlayerInteraction-komponenten ikke fundet på spilleren.");
                enabled = false;
            }
        }
        else
        {
            Debug.LogError("Intet GameObject med tagget 'Player' fundet i scenen.");
            enabled = false;
        }
    }

    // Tjekker om pickup-objektet er inden for en radius omkring midten af skærmen
    public bool IsInView(Camera camera)
    {
        if (camera == null) return false;
        Vector3 viewportPoint = camera.WorldToViewportPoint(transform.position);

        // viewportPoint.z > 0 sikrer at objektet er foran kameraet
        if (viewportPoint.z > 0)
        {
            // Beregn afstanden fra midten af skærmen (0.5, 0.5) til objektets viewport-position
            float distanceToCenter = Vector2.Distance(new Vector2(viewportPoint.x, viewportPoint.y), new Vector2(0.5f, 0.5f));
            return distanceToCenter <= pickupRadius;
        }

        return false;
    }

    // Beregner afstanden til spilleren
    public float DistanceToPlayer()
    {
        if (playerTransform == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, playerTransform.position);
    }

    // Valgfrit: Visuel feedback i editoren af pickupRadius
    private void OnDrawGizmosSelected()
    {
        // Tegn en wire sphere omkring objektet for at indikere interactionRange
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // **Denne gizmo er til at visualisere pickupRadius i viewport space, hvilket er sværere at visualisere direkte i verdenen.**
        // Du kan evt. tegne en kegle fra kameraet i editoren for at visualisere synsfeltet og pickupRadius.
    }


}

