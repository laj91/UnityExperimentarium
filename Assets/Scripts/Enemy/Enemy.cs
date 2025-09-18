using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 lastKnownPos;

    public NavMeshAgent Agent { get => agent; }
    public Transform Player { get => player; }
    public Vector3 LastKnownPos { get => lastKnownPos; set => lastKnownPos = value; }

    public Path path;
    [Header("Sight Values")]
    public float sightDistance = 20f;
    public float fieldOfView = 85f;
    public float eyeHeight;
    [Header("Weapon Values")]
    public Transform gunBarrel;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    [Range(0.1f, 10f)]
    public float fireRate;
    public GameObject debugSphere;
    // Just for debugging purposes.
    [SerializeField]
    private string currentState;


    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Inisialize();
        if (player == null)
        {
            Debug.Log("Found player");
            player = Camera.main.transform;
        }

        if (agent.isOnNavMesh) // Undgå fejl hvis den starter uden for NavMesh
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        CanSeePlayer();
        currentState = stateMachine.activeState.ToString();
        debugSphere.transform.position = lastKnownPos;
    }

    public bool CanSeePlayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position - (Vector3.up * eyeHeight);
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    Ray ray = new Ray(transform.position + (Vector3.up * eyeHeight), targetDirection);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, sightDistance))
                    {
                        Debug.Log($"Raycast hit: {hitInfo.transform.gameObject.name}");
                        if (hitInfo.transform.gameObject.CompareTag("Player")) // Tjekker tag
                        {
                            Debug.Log("Player is visible!");
                            return true;
                        }
                        else
                        {
                            Debug.Log("Raycast hit something other than the player.");
                        }
                    }
                    Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.red);
                }
            }
        }
        return false;
    }




}
