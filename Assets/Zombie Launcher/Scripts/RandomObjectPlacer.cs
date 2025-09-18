using UnityEngine;

public class RandomObjectPlacer : MonoBehaviour
{
    public GameObject[] prefabs; // Drag prefabs from "low poly construction" here in the Inspector
    public int numberOfObjects = 20; // Number of objects to place
    public Vector3 placementArea = new Vector3(50, 0, 50); // Area to place objects (X, Y, Z)
    public float raycastHeight = 100f; // Height from which to cast rays downward
    public float surfaceOffset = 0.1f; // Small offset above the surface to avoid clipping
    public int maxPlacementAttempts = 50; // Maximum attempts to find a valid placement per object

    private int constructionLayer;
    private LayerMask constructionLayerMask;

    void Start()
    {
        constructionLayer = LayerMask.NameToLayer("ConstructionLayer");
        if (constructionLayer == -1)
        {
            Debug.LogError("ConstructionLayer does not exist. Please create it in the Layers settings.");
            return;
        }

        constructionLayerMask = 1 << constructionLayer;
        PlaceObjects();
    }

    void PlaceObjects()
    {
        //Debug.Log($"Placing {numberOfObjects} objects on ConstructionLayer surfaces in area: {placementArea}");
        
        int successfulPlacements = 0;
        
        for (int i = 0; i < numberOfObjects; i++)
        {
            if (prefabs.Length == 0)
            {
                Debug.LogError("No prefabs assigned to RandomObjectPlacer. Please assign prefabs in the Inspector.");
                return;
            }

                    GameObject prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Length)];

            RaycastHit hit;
            if (FindValidPlacementPosition(out hit))
            {
                Vector3 surfacePoint = hit.point;
                Vector3 surfaceNormal = hit.normal;

                Quaternion rotation = prefab.transform.rotation;

                GameObject instance = Instantiate(prefab, surfacePoint, rotation);
                LiftObjectAboveSurface(instance, surfaceNormal, surfacePoint);

                successfulPlacements++;
                //Debug.Log($"Placed object {successfulPlacements}/{numberOfObjects} at position: {instance.transform.position}");
            }
            else
            {
                Debug.LogWarning($"Failed to find valid placement for object {i + 1}. Skipping.");
            }
        }

        Debug.Log($"Successfully placed {successfulPlacements}/{numberOfObjects} objects.");
    }

    bool FindValidPlacementPosition(out RaycastHit hit)
    {
        hit = default;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3 randomHorizontalPosition = new Vector3(
                UnityEngine.Random.Range(-placementArea.x / 2, placementArea.x / 2),
                0,
                UnityEngine.Random.Range(-placementArea.z / 2, placementArea.z / 2)
            );

            Vector3 raycastStart = transform.position + randomHorizontalPosition + Vector3.up * raycastHeight;

            if (Physics.Raycast(raycastStart, Vector3.down, out hit, raycastHeight * 2f, constructionLayerMask))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void LiftObjectAboveSurface(GameObject go, Vector3 surfaceNormal, Vector3 surfacePoint)
    {
        Bounds bounds = CalculateWorldBounds(go);
        if (bounds.size == Vector3.zero)
        {
            go.transform.position = surfacePoint + surfaceNormal * surfaceOffset;
            return;
        }

        float support = GetAabbHalfSizeAlong(bounds, surfaceNormal);
        float minAlongN = Vector3.Dot(bounds.center, surfaceNormal) - support;
        float targetMin = Vector3.Dot(surfacePoint, surfaceNormal) + surfaceOffset;
        float delta = targetMin - minAlongN;

        go.transform.position += surfaceNormal * delta;
    }

    Bounds CalculateWorldBounds(GameObject go)
    {
        var colliders = go.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            Bounds b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                b.Encapsulate(colliders[i].bounds);
            return b;
        }

        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        return new Bounds(go.transform.position, Vector3.zero);
    }

    float GetAabbHalfSizeAlong(Bounds bounds, Vector3 dir)
    {
        Vector3 e = bounds.extents;
        Vector3 a = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
        return e.x * a.x + e.y * a.y + e.z * a.z;
    }

    // Draw placement area gizmos in the Scene view (when this object is selected)
    void OnDrawGizmosSelected()
    {
        // Ground-level placement area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, placementArea);

        // Raycast start area at raycastHeight
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * raycastHeight, placementArea);

        // Line indicating raycast center
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawLine(transform.position + Vector3.up * raycastHeight, transform.position);
    }
}

