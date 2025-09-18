using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    public float lookSpeed = 5.0f;

    private NavMeshAgent agent;
    private float nextAttackTime = 0f;
    private EnemyHealth enemyHealth; // Bruger nu EnemyHealth til at tjekke states
    private Animator animator; // Reference for animation

    void Start()
    {
        // Hent referencer fra EnemyHealth eller GetComponent
        enemyHealth = GetComponent<EnemyHealth>();
        agent = enemyHealth.agent; // F� fra EnemyHealth
        animator = enemyHealth.animator; // F� fra EnemyHealth

        if (player == null)
        {
            player = Camera.main.transform;
        }

        // Stopafstand s�ttes stadig, men agenten kan blive deaktiveret
        if (agent.isOnNavMesh) // Undg� fejl hvis den starter uden for NavMesh
        {
            agent.stoppingDistance = attackRange * 0.8f;
        }
    }

    void Update()
    {
        // G�r INTET hvis fjenden er d�d eller ragdolled
        if (enemyHealth == null || enemyHealth.IsDead() || enemyHealth.IsRagdolled())
        {
            // S�rg for at agenten er stoppet hvis den er aktiv men burde v�re ragdolled/d�d
            // (Selvom EnemyHealth burde have deaktiveret den)
            if (agent.enabled && !agent.isStopped) agent.isStopped = true;
            return;
        }

        // Tjek om agenten er aktiv (den kan v�re deaktiveret under recovery)
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            // Vent p� at agenten bliver klar igen efter recovery
            return;
        }


        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Drej mod spilleren (kun hvis ikke allerede ved at angribe eller lignende)
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }


        if (distanceToPlayer <= agent.stoppingDistance) // Brug agentens stopafstand
        {
            agent.isStopped = true; // Stop bev�gelse
            animator?.SetBool("IsWalking", false); // Stop g�-animation

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // Bev�g mod spilleren hvis agenten er klar
            if (agent.isOnNavMesh)
            { // Ekstra sikkerhedstjek
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator?.SetBool("IsWalking", true); // Start g�-animation
            }
        }
    }

    void Attack()
    {
        Debug.Log("Fjende angriber!");
        animator?.SetTrigger("Attack"); // Start angrebs-animation

        // Skadeslogik (som f�r, evt. med raycast eller animation event)
        // PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        // if (playerHealth != null) { playerHealth.TakeDamage(attackDamage); }
    }
}