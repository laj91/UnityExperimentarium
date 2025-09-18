using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RagdollHelper : MonoBehaviour
{
    private Animator animator;
    public Rigidbody[] rigidbodies;
    public Collider[] colliders;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (rigidbodies == null)
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        if (colliders == null)
        colliders = GetComponentsInChildren<Collider>();

        // Deaktiver ragdoll ved start
        SetRagdollState(false);
    }

    public void SetRagdollState(bool isRagdoll)
    {
        if (animator != null)
        {
            animator.enabled = !isRagdoll;
        }

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !isRagdoll;
            if (isRagdoll)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate; // For smoothere fysik
                Debug.Log("Ragdoll active: " + isRagdoll);
            }
            else
            {
                //rb.interpolation = RigidbodyInterpolation.None; // Mindre CPU n�r animeret
                //rb.linearVelocity = Vector3.zero;
                //rb.angularVelocity = Vector3.zero;
                Debug.Log("Ragdoll not active: " + isRagdoll);
            }
        }

        foreach (Collider col in colliders)
        {
            if (col.GetComponent<XRGrabInteractable>() != null)
            {
                col.enabled = true;
            }
            else 
            {
                col.enabled = isRagdoll;
            }
                
        }
    }

    // Bruges til at anvende en kraft p� en specifik del af ragdollen
    public void ApplyForce(Rigidbody bodyPart, Vector3 force, ForceMode mode)
    {
        if (bodyPart != null && bodyPart.isKinematic == false)
        {
            bodyPart.AddForce(force, mode);
        }
    }

    // Bruges til at finde en specifik rigidbody i ragdollen (f.eks. ved navn)
    public Rigidbody GetRigidbody(string name)
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb.name == name)
            {
                return rb;
            }
        }
        return null;
    }
}