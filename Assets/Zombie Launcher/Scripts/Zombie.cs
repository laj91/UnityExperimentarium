using System.Linq;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    private enum ZombieState
    {
        Idle,
        Walking,
        Attacking,
        Dying,
        Ragdoll
    }
    [SerializeField] private ZombieState _currentState = ZombieState.Walking;
    [SerializeField] private Camera _camera;
    Rigidbody[] _ragdollRigidbodies;
    private Animator _animator;
    private CharacterController _characterController;

    private void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody item in _ragdollRigidbodies)
        {
            Debug.Log("Rigidbodies found: " + item.gameObject.name);
        }
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        DisableRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentState)
        {
            case ZombieState.Idle:
                break;
            case ZombieState.Walking:
                WalkingBehaviour();
                break;
            case ZombieState.Attacking:
                break;
            case ZombieState.Dying:
                break;
            case ZombieState.Ragdoll:
                RagdollBehavior();
                break;
            default:
                break;
        }
       
    }
    private Rigidbody FindHitRigidbody(Vector3 hitPoint)
    {
        Rigidbody closestRigidbody = null;
        float closestDistance = 0;

        foreach (var rigidbody in _ragdollRigidbodies)
        {
            float distance = Vector3.Distance(rigidbody.position, hitPoint);

            if (closestRigidbody == null || distance < closestDistance)
            {
                closestDistance = distance;
                closestRigidbody = rigidbody;
            }
        }

        return closestRigidbody;
    }
    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();

        Rigidbody hitRigidbody = FindHitRigidbody(hitPoint);

        hitRigidbody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        _currentState = ZombieState.Ragdoll;
    }
    private void DisableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        _characterController.enabled = true;
        _animator.enabled = true;
    }

    private void EnableRagdoll()
    {
        foreach (var rigidbody in _ragdollRigidbodies)
        {
            rigidbody.isKinematic = false;
        }
        _characterController.enabled = false;
        _animator.enabled = false;
    }

    private void WalkingBehaviour()
    {
        Vector3 direction = _camera.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 20 * Time.deltaTime);
    }

    private void RagdollBehavior()
    {
        Debug.Log("Ragdoll behavior active. Press Space to return to walking state.");
    }

}
