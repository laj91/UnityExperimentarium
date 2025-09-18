using UnityEngine;
using UnityEngine.SceneManagement;

public class CubeWallGenerator : MonoBehaviour
{
    [Header("Cube Settings")]
    [SerializeField] GameObject cubePrefab;
    [SerializeField] int width = 5;
    [SerializeField] int height = 5;
    [SerializeField] float cubeSize = 0.5f;

    [Header("Wall Origin Offset")]
    [SerializeField] Vector3 startOffset = Vector3.zero;

    private Rigidbody[] cubeRigidBodies;

    void Start()
    {
        GenerateWall();
    }

   

    void GenerateWall()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab is not assigned!");
            return;
        }

        int total = Mathf.Max(0, width) * Mathf.Max(0, height);
        cubeRigidBodies = new Rigidbody[total];

        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 position = transform.position + startOffset
                    + transform.right * (x * cubeSize)
                    + transform.up * (y * cubeSize);

                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                cube.transform.localScale = Vector3.one * cubeSize;

                Rigidbody rb = cube.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    if (index < cubeRigidBodies.Length)
                        cubeRigidBodies[index++] = rb;
                    rb.isKinematic = true;
                }
                else
                {
                    Debug.LogWarning("Cube Prefab does not have a Rigidbody component!");
                }

                // Tilføj collision handler til hver cube
                if (cube.GetComponent<CubeCollisionHandler>() == null)
                {
                    cube.AddComponent<CubeCollisionHandler>();
                }
            }
        }
    }

    
    // Optional helper to later enable physics
    public void ActivatePhysics()
    {
        if (cubeRigidBodies == null) return;
        foreach (var rb in cubeRigidBodies)
        {
            if (rb != null) rb.isKinematic = false;
        }
    }
}
