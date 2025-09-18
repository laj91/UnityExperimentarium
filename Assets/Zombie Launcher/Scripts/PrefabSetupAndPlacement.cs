// 21-08-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEditor;
using UnityEngine;

public class PrefabSetupAndPlacement : MonoBehaviour
{
    public GameObject[] prefabs; // Drag prefabs here in Inspector
    public int numberOfObjects = 10; // Number of objects to place
    public Vector3 placementArea = new Vector3(50, 0, 50); // Area for placement
    public LayerMask placementLayer; // Layer for placement

    void Start()
    {
        foreach (GameObject prefab in prefabs)
        {
            // Update prefab with collider and rigidbody
            if (prefab.GetComponent<Collider>() == null)
            {
                UpdatePrefab(prefab);
            }
           
            // Place objects in the scene
            PlaceObjects(prefab);
        }
    }

    void UpdatePrefab(GameObject prefab)
    {
        // Check if prefab already has a collider
        if (prefab.GetComponent<Collider>() == null)
        {
            prefab.AddComponent<BoxCollider>();
        } else
        {
            // Optionally, you can log or handle existing colliders
            Debug.Log($"Prefab {prefab.name} already has a collider.");
        }

        // Check if prefab already has a rigidbody
        if (prefab.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = prefab.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Make it kinematic if necessary
        } else
        {
            // Optionally, you can log or handle existing rigidbodies
            Debug.Log($"Prefab {prefab.name} already has a rigidbody.");
        }

        // Ensure the rotation is correct
        prefab.transform.rotation = Quaternion.identity;

        Debug.Log($"Updated prefab: {prefab.name}");
    }

    void PlaceObjects(GameObject prefab)
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Generate a random position within the area
            Vector3 randomPosition = new Vector3(
                Random.Range(-placementArea.x / 2, placementArea.x / 2),
                0, // Adjust Y if necessary
                Random.Range(-placementArea.z / 2, placementArea.z / 2)
            );

            // Check if the position is valid using raycast
            if (Physics.Raycast(randomPosition + Vector3.up * 10, Vector3.down, out RaycastHit hit, Mathf.Infinity, placementLayer))
            {
                // Place the object at the hit point
                GameObject instance = Instantiate(prefab, hit.point, Quaternion.identity);
                instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal); // Adjust rotation to surface
                Debug.Log($"Placed {prefab.name} at {hit.point}");
            }
            else
            {
                Debug.LogWarning($"Failed to find valid placement for {prefab.name}. Skipping.");
            }
        }
    }
}
