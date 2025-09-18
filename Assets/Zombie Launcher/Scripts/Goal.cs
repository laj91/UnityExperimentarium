using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("Køres efter GameManager har registreret målet.")]
    public UnityEvent onGoalRegistered;

    [Header("References (valgfri – auto-finder hvis tom)")]
    [SerializeField] private GameManager gameManager;

    [Header("Behaviour")]
    [SerializeField] private bool disableColliderOnHit = true;
    [SerializeField] private bool stickRagdollToGoal = false;
    [SerializeField] private bool freezeRagdollRigidbodies = true;

    private bool consumed = false;
    private Collider goalCollider;

    private void Awake()
    {
        goalCollider = GetComponent<Collider>();
        if (!goalCollider.isTrigger)
            goalCollider.isTrigger = true;

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            Debug.LogWarning($"Goal '{name}' fandt ingen GameManager i scenen.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag("RagdollBullet")) return;
        if (gameManager != null && gameManager.LevelEnded) return;

        consumed = true; // Lås straks (beskyt mod flere ragdoll colliders)

        // Registrér i GameManager (stopper evt. level hvis dette var sidste)
        if (gameManager != null)
        {
            gameManager.RegisterGoalHit();
        }

        // Valgfrit: hæft/stop ragdoll
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            if (freezeRagdollRigidbodies)
            {
                // Frys alle limbs
                foreach (var limb in rb.GetComponentsInChildren<Rigidbody>())
                {
                    limb.linearVelocity = Vector3.zero;
                    limb.angularVelocity = Vector3.zero;
                    limb.isKinematic = true;
                }
            }
            else
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (stickRagdollToGoal)
            {
                other.transform.SetParent(transform, true);
            }
        }

        if (disableColliderOnHit && goalCollider != null)
            goalCollider.enabled = false;

        // Evt. visuel / lyd feedback kan bindes i Inspector
        onGoalRegistered?.Invoke();

        Debug.Log($"Goal '{name}' ramtes og er nu registreret.");
    }
    public void ForceDisable()
    {
        if (goalCollider) goalCollider.enabled = false;
        consumed = true;
    }
}
