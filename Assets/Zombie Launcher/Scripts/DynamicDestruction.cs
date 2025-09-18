using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DynamicDestruction : MonoBehaviour
{
    [SerializeField] private float health = 500;
    [SerializeField] private float destroyForce = 2f;
    [SerializeField] private float reenableCollisionDelay = 2f;
    [SerializeField] private float reenableFragmentCollisionDelay = 2f;
    [SerializeField] private string fragmentLayerName = "Fragment";
    private static bool fragmentCollisionIgnored = false;

    private Rigidbody rb;
    private Collider col;
    private bool destroyed = false;

    void Awake()
    {
        // Indstil layer på hele fragmentet og eventuelle children
        int layer = LayerMask.NameToLayer(fragmentLayerName);
        SetLayerRecursively(gameObject, layer);

        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        

    }

    void Start()
    {
        if (!fragmentCollisionIgnored)
        {
            int layer = LayerMask.NameToLayer(fragmentLayerName);
            Physics.IgnoreLayerCollision(layer, layer, true);
            fragmentCollisionIgnored = true;
        }
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }


    public void SetHealth(float value)
    {
        health = value;
    }

    public void SetForce(float value)
    {
        destroyForce = value;
    }

    public void SetDelay(float value)
    {
        reenableCollisionDelay = value;
    }

    public void TakeDamage(float amount)
    {
        if (destroyed) return;

        health -= amount;
        if (health <= 0f)
        {
            DestroyFragment();
        }
    }

    private void DestroyFragment()
    {
        if (destroyed) return;

        destroyed = true;

        // Ignorér fragment-fragment kollisioner globalt
        int layer = LayerMask.NameToLayer(fragmentLayerName);
        Physics.IgnoreLayerCollision(layer, layer, true);
        StartCoroutine(ReenableFragmentCollisionsDelayed(layer, reenableFragmentCollisionDelay));

        StartCoroutine(EnablePhysicsDelayed());
    }
    private IEnumerator ReenableFragmentCollisionsDelayed(int layer, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreLayerCollision(layer, layer, false);
    }

    private IEnumerator EnablePhysicsDelayed()
    {
        if (!rb.isKinematic) yield break;

        // Vent 2 physics frames for at undgå clipping
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 1f;
        rb.centerOfMass = Vector3.zero;

        // Giv fragmentet et lille puff i en tilfældig retning
        Vector3 randomDir = Random.onUnitSphere;
        rb.AddForce(randomDir * destroyForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Hvis ragdollen rammer dette fragment
        if (collision.gameObject.CompareTag("Projectile"))
        {
            col.isTrigger = false;
            // Midlertidigt ignorér kollision for at undgå glitchet fysik
            Physics.IgnoreCollision(col, collision.collider, true);
            StartCoroutine(ReenableCollision(collision.collider));

            TakeDamage(10f); // Du kan justere denne værdi
        }

        // Hvis du skyder med et projektil
        //if (collision.gameObject.CompareTag("Projectile"))
        //{
        //    TakeDamage(5f);
        //}
    }

    private IEnumerator ReenableCollision(Collider other)
    {
        yield return new WaitForSeconds(reenableCollisionDelay);
        if (col != null && other != null)
        {
            Physics.IgnoreCollision(col, other, false);
        }
    }


}
