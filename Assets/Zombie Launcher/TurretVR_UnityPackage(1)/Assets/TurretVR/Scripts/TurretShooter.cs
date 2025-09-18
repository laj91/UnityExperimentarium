
using UnityEngine;
using UnityEngine.InputSystem;

public class TurretShooter : MonoBehaviour
{
    public Transform firePoint;
    public GameObject projectilePrefab;
    public InputActionProperty rightTrigger;

    public float projectileSpeed = 30f;

    void Update()
    {
        if (rightTrigger.action.WasPressedThisFrame())
        {
            Fire();
        }
    }

    void Fire()
    {
        var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        //var rb = projectile.GetComponent<Rigidbody>();
        foreach (var rb in projectile.GetComponentsInChildren<Rigidbody>())
        {
            if (rb != null)
            {
                rb.isKinematic = false; // Ensure the rigidbody is not kinematic
                rb.useGravity = true; // Enable gravity for the projectile
                rb.linearVelocity = firePoint.forward * projectileSpeed;
            }
        }
    }
}
