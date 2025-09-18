using UnityEngine;
using UnityEngine.AI;
using System.Collections; // N�dvendigt for Coroutines

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float knockdownForceThreshold = 10f; // Hvor kraftigt et st�d skal der til for at v�lte? (Justeres!)
    public float ragdollDuration = 3.0f; // Hvor l�nge skal fjenden v�re ragdolled efter et st�d (f�r den pr�ver at rejse sig)?
    public float recoveryCheckDelay = 0.5f; // Hvor ofte tjekkes om fjenden kan rejse sig?
    public float stableVelocityThreshold = 0.2f; // Hvor lav skal hastigheden v�re f�r fjenden anses for stabil nok til at rejse sig?
    public Transform ragdollRootBone; // VIGTIGT: S�t denne til hoften/roden af din ragdoll i Inspectoren!
    public Vector3 velocity = Vector3.zero;

    [Header("Component References")]
    public Animator animator; // Tilf�j reference til Animator
    public NavMeshAgent agent; // Tilf�j reference til NavMeshAgent
    public Rigidbody mainRigidbody; // Fjendens prim�re Rigidbody (den der IKKE er en del af ragdoll-lemmerne)
    public Collider mainCollider; // Fjendens prim�re Collider

    [Header("Ragdoll Helper")]
    public RagdollHelper ragdollHelper; // Reference til RagdollHelper scriptet

    public int currentHealth;
    private bool isDead = false;
    private bool isRagdolled = false;
    private Coroutine recoveryCoroutine = null; // Til at styre recovery processen

    void Awake() // Brug Awake for at finde komponenter f�r Start k�rer i andre scripts
    {
        currentHealth = maxHealth;

        // Find komponenter hvis de ikke er sat i inspectoren
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (mainRigidbody == null) mainRigidbody = GetComponent<Rigidbody>();
        if (mainCollider == null) mainCollider = GetComponent<Collider>();

        // Tjek om RagdollHelper er sat
        if (ragdollHelper == null)
        {
            ragdollHelper = GetComponent<RagdollHelper>();
            if (ragdollHelper == null)
            {
                Debug.LogError("RagdollHelper scriptet mangler p� dette objekt!", this);
            }
        }

        // Find ragdoll dele automatisk hvis ragdollRootBone er sat (RagdollHelper h�ndterer nu aktivering/deaktivering)
        if (ragdollRootBone != null && ragdollHelper != null)
        {
            if (ragdollHelper.GetComponentsInChildren<Rigidbody>().Length == 0 || ragdollHelper.GetComponentsInChildren<Collider>().Length == 0)
            {
                Debug.LogWarning("Ragdoll dele ikke fundet via RagdollHelper. S�rg for at RagdollHelper er p� samme objekt eller et parent objekt med ragdoll-delene.", this);
            }
        }
        else if (ragdollRootBone == null)
        {
            Debug.LogError("Ragdoll Root Bone er ikke sat p� EnemyHealth scriptet!", this);
        }

        // Start i Animeret tilstand
        DisableRagdoll();
    }

    public void TakeDamage(int damageAmount, Vector3 hitForce = default(Vector3)) // Tilf�jet hitForce
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " tog " + damageAmount + " skade. Resterende liv: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Tjek om slaget var h�rdt nok til at v�lte fjenden
            // Vi bruger hitForce.magnitude, som skal gives fra kilden (f.eks. spillerens slag)
            if (hitForce.magnitude >= knockdownForceThreshold && !isRagdolled)
            {
                TriggerTemporaryRagdoll(hitForce); // V�lt fjenden
            }
            else if (!isRagdolled) // Kun spil "ramt" animation hvis ikke allerede ragdolled
            {
                animator?.SetTrigger("TakeHit");
            }
        }
    }

    // Bruges hvis noget rammer fjenden (f.eks. et kastet objekt eller spillerens slag)
    void OnCollisionEnter(Collision collision)
    {
        if (isDead || isRagdolled) return; // Reager ikke hvis d�d eller allerede ragdolled

        float impactForce = collision.relativeVelocity.magnitude;
        // Tjekker ogs� tags for at undg� at blive v�ltet af sm�ting eller gulvet
        if (impactForce >= knockdownForceThreshold && (collision.gameObject.CompareTag("PlayerHand") || collision.gameObject.CompareTag("PhysicsObject")))
        {
            // Beregn en kraft baseret p� kollisionen
            Vector3 forceDirection = collision.contacts[0].point - transform.position; // Retning fra center til kontaktpunkt
            forceDirection = -forceDirection.normalized; // Kraft v�k fra kontaktpunktet
            TriggerTemporaryRagdoll(forceDirection * impactForce); // Anvend kraften
        }
    }


    public void TriggerTemporaryRagdoll(Vector3 force)
    {
        if (isDead || isRagdolled || ragdollHelper == null) return; // Kan ikke v�ltes hvis d�d, allerede v�ltet eller RagdollHelper mangler

        Debug.Log("Triggering temporary ragdoll!");
        EnableRagdoll();

        // Anvend den indkommende kraft p� rod-knoglen via RagdollHelper
        Rigidbody rootRb = ragdollHelper.GetRigidbody(ragdollRootBone.name);
        if (rootRb != null)
        {
            ragdollHelper.ApplyForce(rootRb, force, ForceMode.Impulse); // Giv den et skub
        }
        else
        {
            Debug.LogWarning("Could not find root rigidbody in RagdollHelper using name: " + ragdollRootBone.name, this);
        }

        // Start coroutine for at fors�ge recovery efter en tid
        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
        recoveryCoroutine = StartCoroutine(RecoverFromRagdollTimer(ragdollDuration));
    }

    IEnumerator RecoverFromRagdollTimer(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Begynd at tjekke om fjenden er stabil nok til at rejse sig
        while (isRagdolled && !isDead && ragdollHelper != null)
        {
            Rigidbody rootRb = ragdollHelper.GetRigidbody(ragdollRootBone.name);
            if (rootRb != null && rootRb.linearVelocity.magnitude < stableVelocityThreshold)
            {
                // Stabil nok, fors�g at rejse dig
                // Vent et kort �jeblik mere for at sikre stabilitet
                yield return new WaitForSeconds(0.2f);
                if (rootRb.linearVelocity.magnitude < stableVelocityThreshold) // Dobbelttjek
                {
                    TryRecover();
                    yield break; // Stop coroutine'en n�r recovery startes
                }
            }
            // Vent f�r n�ste tjek
            yield return new WaitForSeconds(recoveryCheckDelay);
        }
        recoveryCoroutine = null; // Ryd op
    }

    void TryRecover()
    {
        if (isDead || ragdollHelper == null) return; // Kan ikke rejse sig hvis d�d eller RagdollHelper mangler

        Debug.Log("Attempting recovery from ragdoll...");

        // 1. Gem ragdoll position/rotation (specifikt hofte/rod knoglen)
        Rigidbody rootRb = ragdollHelper.GetRigidbody(ragdollRootBone.name);
        if (rootRb == null)
        {
            Debug.LogWarning("Could not find root rigidbody for recovery.", this);
            DisableRagdoll(); // Sikkerhedsm�ssigt, sl� ragdoll fra hvis vi ikke kan finde roden
            return;
        }
        Vector3 recoveryPosition = rootRb.position;
        Quaternion recoveryRotation = rootRb.rotation;

        // 2. Deaktiver Ragdoll
        DisableRagdoll();

        // 3. Juster hoved-transform til ragdoll position/rotation
        //    L�ft positionen lidt for at undg� at sidde fast i gulvet
        transform.position = recoveryPosition + Vector3.up * 0.1f;
        //    Juster Y-rotationen s� fjenden kigger fremad ift. hoften, men st�r oprejst
        transform.rotation = Quaternion.Euler(0f, recoveryRotation.eulerAngles.y, 0f);


        // 4. Nulstil NavMeshAgent position (vigtigt!)
        //    Warp flytter agenten �jeblikkeligt uden at beregne sti
        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
            agent.isStopped = false; // Lad den begynde at bev�ge sig igen
        }
        else if (agent != null)
        {
            // Hvis den landede uden for NavMesh, er der et problem.
            // M�ske teleporter til n�rmeste NavMesh punkt, eller bare d�?
            Debug.LogWarning("Enemy landed outside NavMesh during recovery. Killing enemy.");
            Die(); // Simpel l�sning: dr�b fjenden hvis den lander forkert.
            return;
        }


        // 5. Spil "Get Up" animation
        //    Du skal have en animation (f.eks. "GetUpFromBelly", "GetUpFromBack")
        //    og logik til at v�lge den rigtige baseret p� ragdollRootBone's orientering.
        //    Simpel l�sning: Spil bare �n "GetUp" trigger.
        animator?.SetTrigger("GetUp"); // S�rg for at have en "GetUp" trigger i din Animator Controller

        // 6. Genaktiver AI scriptet (hvis det blev deaktiveret)
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null) aiScript.enabled = true;

        Debug.Log("Recovery sequence initiated.");
    }


    void EnableRagdoll()
    {
        if (isDead || isRagdolled || ragdollHelper == null) return; // Intet at g�re hvis d�d, allerede ragdolled eller RagdollHelper mangler

        isRagdolled = true;

        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine); // Stop evt. recovery fors�g
        recoveryCoroutine = null;

        // Brug RagdollHelper til at skifte til ragdoll tilstand
        ragdollHelper.SetRagdollState(true);

        animator.enabled = false; // Sl� animation fra (RagdollHelper deaktiverer ogs� animatoren)
        if (agent != null) agent.enabled = false; // Sl� navigation fra
        if (mainRigidbody != null) mainRigidbody.isKinematic = true; // G�r hoved-rigidbody passiv
        if (mainCollider != null) mainCollider.enabled = false; // Sl� hoved-collider fra for at undg� konflikter

        Debug.Log("Ragdoll Enabled (via RagdollHelper)");
    }

    void DisableRagdoll()
    {
        if (isDead || !isRagdolled || ragdollHelper == null) return; // Intet at g�re hvis d�d, ikke ragdolled eller RagdollHelper mangler

        isRagdolled = false;

        // Brug RagdollHelper til at skifte tilbage til animeret tilstand
        ragdollHelper.SetRagdollState(false);

        if (animator != null) animator.enabled = true;
        if (mainRigidbody != null) mainRigidbody.isKinematic = false; // Eller som den var f�r
        if (mainCollider != null) mainCollider.enabled = true;

        // T�nd for NavMeshAgent *kun* hvis fjenden ikke er d�d
        if (!isDead && agent != null)
        {
            // VIGTIGT: Agenten skal m�ske flyttes til ragdollens position F�R den genaktiveres.
            // Dette h�ndteres nu i TryRecover()
            agent.enabled = true;
        }

        Debug.Log("Ragdoll Disabled (via RagdollHelper)");
    }

    void Die()
    {
        if (isDead) return; // Allerede d�d

        isDead = true;
        currentHealth = 0;
        Debug.Log(gameObject.name + " d�de.");

        // Aktiver ragdoll permanent ved d�d via RagdollHelper
        if (ragdollHelper != null)
        {
            ragdollHelper.SetRagdollState(true);
        }
        else
        {
            Debug.LogError("RagdollHelper mangler, permanent ragdoll ved d�d kunne ikke aktiveres.", this);
        }

        // Deaktiver AI scriptet permanent
        EnemyAI aiScript = GetComponent<EnemyAI>();
        if (aiScript != null) aiScript.enabled = false;

        // G�r hoved-objektet klar til evt. despawn
        // Destroy(gameObject, despawnDelay); // Hvis du vil fjerne liget
    }

    // Funktion til at tjekke tilstand udefra
    public bool IsDead() => isDead;
    public bool IsRagdolled() => isRagdolled;

    // Kald denne fra dit VR Interaction script n�r en h�nd slipper en ragdoll del
    public void NotifyRagdollReleased()
    {
        if (isRagdolled && ragdollHelper != null && ragdollRootBone != null)
        {
            Debug.Log("Ragdoll released with velocity: " + velocity);
            Rigidbody rootRb = ragdollHelper.GetRigidbody(ragdollRootBone.name);
            if (rootRb != null)
            {
                ragdollHelper.ApplyForce(rootRb, velocity, ForceMode.VelocityChange); // Overf�r kaste-hastigheden
            }
            else
            {
                Debug.LogWarning("Could not find root rigidbody in RagdollHelper for release force.", this);
            }
            // Genstart recovery timer, nu hvor den er blevet kastet
            if (!isDead)
            {
                if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = StartCoroutine(RecoverFromRagdollTimer(ragdollDuration)); // Start forfra
            }
        }
    }
    // Kald denne fra dit VR Interaction script n�r en h�nd griber en ragdoll del
    public void NotifyRagdollGrabbed()
    {
        if (!isDead && ragdollHelper != null)
        {
            if (!isRagdolled)
            {
                // Hvis fjenden gribes mens den er aktiv, tving den i ragdoll
                Debug.Log("Grabbed while active, forcing ragdoll.");
                EnableRagdoll();
                // Stop recovery hvis den var i gang
                if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }
            else
            {
                // Hvis den allerede er ragdolled og gribes, stop recovery processen midlertidigt
                Debug.Log("Grabbed while ragdolled, pausing recovery.");
                if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
                recoveryCoroutine = null;
            }
        }
    }
}

//using UnityEngine;
//using UnityEngine.AI;
//using System.Collections; // N�dvendigt for Coroutines

//public class EnemyHealth : MonoBehaviour
//{
//    public int maxHealth = 100;
//    public float knockdownForceThreshold = 10f; // Hvor kraftigt et st�d skal der til for at v�lte? (Justeres!)
//    public float ragdollDuration = 3.0f; // Hvor l�nge skal fjenden v�re ragdolled efter et st�d (f�r den pr�ver at rejse sig)?
//    public float recoveryCheckDelay = 0.5f; // Hvor ofte tjekkes om fjenden kan rejse sig?
//    public float stableVelocityThreshold = 0.2f; // Hvor lav skal hastigheden v�re f�r fjenden anses for stabil nok til at rejse sig?
//    public Transform ragdollRootBone; // VIGTIGT: S�t denne til hoften/roden af din ragdoll i Inspectoren!
//    public Vector3 releaseForce = Vector3.zero;

//    [Header("Component References")]
//    public Animator animator; // Tilf�j reference til Animator
//    public NavMeshAgent agent; // Tilf�j reference til NavMeshAgent
//    public Rigidbody mainRigidbody; // Fjendens prim�re Rigidbody (den der IKKE er en del af ragdoll-lemmerne)
//    public Collider mainCollider; // Fjendens prim�re Collider

//    [Header("Ragdoll Components (Auto-find or assign)")]
//    public Rigidbody[] ragdollRigidbodies;
//    public Collider[] ragdollColliders;

//    public int currentHealth;
//    private bool isDead = false;
//    private bool isRagdolled = false;
//    private Coroutine recoveryCoroutine = null; // Til at styre recovery processen

//    void Awake() // Brug Awake for at finde komponenter f�r Start k�rer i andre scripts
//    {
//        currentHealth = maxHealth;

//        // Find komponenter hvis de ikke er sat i inspectoren
//        if (animator == null) animator = GetComponentInChildren<Animator>();
//        if (agent == null) agent = GetComponent<NavMeshAgent>();
//        if (mainRigidbody == null) mainRigidbody = GetComponent<Rigidbody>();
//        if (mainCollider == null) mainCollider = GetComponent<Collider>();

//        // Find ragdoll dele automatisk hvis ragdollRootBone er sat
//        if (ragdollRootBone != null)
//        {
//            if (ragdollRigidbodies == null || ragdollRigidbodies.Length == 0)
//                ragdollRigidbodies = ragdollRootBone.GetComponentsInChildren<Rigidbody>();
//            if (ragdollColliders == null || ragdollColliders.Length == 0)
//                ragdollColliders = ragdollRootBone.GetComponentsInChildren<Collider>();
//        }
//        else
//        {
//            Debug.LogError("Ragdoll Root Bone er ikke sat p� EnemyHealth scriptet!", this);
//        }

//        // Start i Animeret tilstand
//        DisableRagdoll();
//    }

//    public void TakeDamage(int damageAmount, Vector3 hitForce = default(Vector3)) // Tilf�jet hitForce
//    {
//        if (isDead) return;

//        currentHealth -= damageAmount;
//        Debug.Log(gameObject.name + " tog " + damageAmount + " skade. Resterende liv: " + currentHealth);

//        if (currentHealth <= 0)
//        {
//            Die();
//        }
//        else
//        {
//            // Tjek om slaget var h�rdt nok til at v�lte fjenden
//            // Vi bruger hitForce.magnitude, som skal gives fra kilden (f.eks. spillerens slag)
//            if (hitForce.magnitude >= knockdownForceThreshold && !isRagdolled)
//            {
//                TriggerTemporaryRagdoll(hitForce); // V�lt fjenden
//            }
//            else if (!isRagdolled) // Kun spil "ramt" animation hvis ikke allerede ragdolled
//            {
//                animator?.SetTrigger("TakeHit");
//            }
//        }
//    }

//    // Bruges hvis noget rammer fjenden (f.eks. et kastet objekt eller spillerens slag)
//    void OnCollisionEnter(Collision collision)
//    {
//        if (isDead || isRagdolled) return; // Reager ikke hvis d�d eller allerede ragdolled

//        float impactForce = collision.relativeVelocity.magnitude;
//        // Tjekker ogs� tags for at undg� at blive v�ltet af sm�ting eller gulvet
//        if (impactForce >= knockdownForceThreshold && (collision.gameObject.CompareTag("PlayerHand") || collision.gameObject.CompareTag("PhysicsObject")))
//        {
//            // Beregn en kraft baseret p� kollisionen
//            Vector3 forceDirection = collision.contacts[0].point - transform.position; // Retning fra center til kontaktpunkt
//            forceDirection = -forceDirection.normalized; // Kraft v�k fra kontaktpunktet
//            TriggerTemporaryRagdoll(forceDirection * impactForce); // Anvend kraften
//        }
//    }


//    public void TriggerTemporaryRagdoll(Vector3 force)
//    {
//        if (isDead || isRagdolled) return; // Kan ikke v�ltes hvis d�d eller allerede v�ltet

//        Debug.Log("Triggering temporary ragdoll!");
//        EnableRagdoll();

//        // Anvend den indkommende kraft p� den ramte del eller roden
//        // Her anvendes den simpelt p� rod-knoglen for nemheds skyld
//        if (ragdollRootBone != null)
//        {
//            Rigidbody rootRb = ragdollRootBone.GetComponent<Rigidbody>();
//            if (rootRb != null)
//            {
//                rootRb.AddForce(force, ForceMode.Impulse); // Giv den et skub
//            }
//        }


//        // Start coroutine for at fors�ge recovery efter en tid
//        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
//        recoveryCoroutine = StartCoroutine(RecoverFromRagdollTimer(ragdollDuration));
//    }

//    IEnumerator RecoverFromRagdollTimer(float delay)
//    {
//        yield return new WaitForSeconds(delay);

//        // Begynd at tjekke om fjenden er stabil nok til at rejse sig
//        while (isRagdolled && !isDead)
//        {
//            if (ragdollRootBone != null)
//            {
//                Rigidbody rootRb = ragdollRootBone.GetComponent<Rigidbody>();
//                if (rootRb != null && rootRb.velocity.magnitude < stableVelocityThreshold)
//                {
//                    // Stabil nok, fors�g at rejse dig
//                    // Vent et kort �jeblik mere for at sikre stabilitet
//                    yield return new WaitForSeconds(0.2f);
//                    if (rootRb.velocity.magnitude < stableVelocityThreshold) // Dobbelttjek
//                    {
//                        TryRecover();
//                        yield break; // Stop coroutine'en n�r recovery startes
//                    }
//                }
//            }
//            // Vent f�r n�ste tjek
//            yield return new WaitForSeconds(recoveryCheckDelay);
//        }
//        recoveryCoroutine = null; // Ryd op
//    }

//    void TryRecover()
//    {
//        if (isDead) return; // Kan ikke rejse sig hvis d�d

//        Debug.Log("Attempting recovery from ragdoll...");

//        // 1. Gem ragdoll position/rotation (specifikt hofte/rod knoglen)
//        Vector3 recoveryPosition = ragdollRootBone.position;
//        Quaternion recoveryRotation = ragdollRootBone.rotation;

//        // 2. Deaktiver Ragdoll
//        DisableRagdoll();

//        // 3. Juster hoved-transform til ragdoll position/rotation
//        //    L�ft positionen lidt for at undg� at sidde fast i gulvet
//        transform.position = recoveryPosition + Vector3.up * 0.1f;
//        //    Juster Y-rotationen s� fjenden kigger fremad ift. hoften, men st�r oprejst
//        transform.rotation = Quaternion.Euler(0f, recoveryRotation.eulerAngles.y, 0f);


//        // 4. Nulstil NavMeshAgent position (vigtigt!)
//        //    Warp flytter agenten �jeblikkeligt uden at beregne sti
//        if (agent.isOnNavMesh)
//        { // Tjek om den nye position er p� navmesh
//            agent.Warp(transform.position);
//            agent.isStopped = false; // Lad den begynde at bev�ge sig igen
//        }
//        else
//        {
//            // Hvis den landede uden for NavMesh, er der et problem.
//            // M�ske teleporter til n�rmeste NavMesh punkt, eller bare d�?
//            Debug.LogWarning("Enemy landed outside NavMesh during recovery. Killing enemy.");
//            Die(); // Simpel l�sning: dr�b fjenden hvis den lander forkert.
//            return;
//        }


//        // 5. Spil "Get Up" animation
//        //    Du skal have en animation (f.eks. "GetUpFromBelly", "GetUpFromBack")
//        //    og logik til at v�lge den rigtige baseret p� ragdollRootBone's orientering.
//        //    Simpel l�sning: Spil bare �n "GetUp" trigger.
//        animator?.SetTrigger("GetUp"); // S�rg for at have en "GetUp" trigger i din Animator Controller

//        // 6. Genaktiver AI scriptet (hvis det blev deaktiveret)
//        EnemyAI aiScript = GetComponent<EnemyAI>();
//        if (aiScript != null) aiScript.enabled = true;

//        Debug.Log("Recovery sequence initiated.");
//    }


//    void EnableRagdoll()
//    {
//        isRagdolled = true;

//        if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine); // Stop evt. recovery fors�g
//        recoveryCoroutine = null;

//        animator.enabled = false; // Sl� animation fra
//        agent.enabled = false; // Sl� navigation fra
//        mainRigidbody.isKinematic = true; // G�r hoved-rigidbody passiv
//        mainCollider.enabled = false; // Sl� hoved-collider fra for at undg� konflikter

//        // Aktiver ragdoll fysik
//        foreach (Rigidbody rb in ragdollRigidbodies)
//        {
//            rb.isKinematic = false;
//            rb.interpolation = RigidbodyInterpolation.Interpolate; // Giver p�nere bev�gelse
//        }
//        foreach (Collider col in ragdollColliders)
//        {
//            col.enabled = true;
//            col.isTrigger = false; // S�rg for at de kolliderer fysisk
//        }
//        Debug.Log("Ragdoll Enabled");
//    }

//    void DisableRagdoll()
//    {
//        isRagdolled = false;

//        // Aktiver "normal" tilstand
//        animator.enabled = true;
//        mainRigidbody.isKinematic = false; // Eller som den var f�r
//        mainCollider.enabled = true;

//        // T�nd for NavMeshAgent *kun* hvis fjenden ikke er d�d
//        if (!isDead)
//        {
//            // VIGTIGT: Agenten skal m�ske flyttes til ragdollens position F�R den genaktiveres.
//            // Dette h�ndteres nu i TryRecover()
//            agent.enabled = true;
//        }


//        // Deaktiver ragdoll fysik
//        foreach (Rigidbody rb in ragdollRigidbodies)
//        {
//            rb.isKinematic = true;
//        }
//        foreach (Collider col in ragdollColliders)
//        {
//            // Beh�ver m�ske ikke at deaktivere colliders, hvis mainCollider klarer det
//            // col.enabled = false;
//        }
//        Debug.Log("Ragdoll Disabled");
//    }

//    void Die()
//    {
//        if (isDead) return; // Allerede d�d

//        isDead = true;
//        currentHealth = 0;
//        Debug.Log(gameObject.name + " d�de.");

//        // Aktiver ragdoll permanent ved d�d
//        EnableRagdoll();

//        // Deaktiver AI scriptet permanent
//        EnemyAI aiScript = GetComponent<EnemyAI>();
//        if (aiScript != null) aiScript.enabled = false;

//        // G�r hoved-objektet klar til evt. despawn
//        // Destroy(gameObject, despawnDelay); // Hvis du vil fjerne liget
//    }

//    // Funktion til at tjekke tilstand udefra
//    public bool IsDead() => isDead;
//    public bool IsRagdolled() => isRagdolled;

//    // Kald denne fra dit VR Interaction script n�r en h�nd slipper en ragdoll del
//    public void NotifyRagdollReleased()
//    {
//        if (isRagdolled && ragdollRootBone != null)
//        {
//            Debug.Log("Ragdoll released with velocity: " + releaseForce);
//            Rigidbody rootRb = ragdollRootBone.GetComponent<Rigidbody>();
//            if (rootRb != null)
//            {
//                rootRb.AddForce(releaseForce, ForceMode.VelocityChange); // Overf�r kaste-hastigheden
//            }
//            // Genstart recovery timer, nu hvor den er blevet kastet
//            if (!isDead)
//            {
//                if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
//                recoveryCoroutine = StartCoroutine(RecoverFromRagdollTimer(ragdollDuration)); // Start forfra
//            }
//        }
//    }
//    // Kald denne fra dit VR Interaction script n�r en h�nd griber en ragdoll del
//    public void NotifyRagdollGrabbed()
//    {
//        if (!isDead && !isRagdolled)
//        {
//            // Hvis fjenden gribes mens den er aktiv, tving den i ragdoll
//            Debug.Log("Grabbed while active, forcing ragdoll.");
//            EnableRagdoll();
//            // Stop recovery hvis den var i gang
//            if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
//            recoveryCoroutine = null;
//        }
//        else if (isRagdolled)
//        {
//            // Hvis den allerede er ragdolled og gribes, stop recovery processen midlertidigt
//            Debug.Log("Grabbed while ragdolled, pausing recovery.");
//            if (recoveryCoroutine != null) StopCoroutine(recoveryCoroutine);
//            recoveryCoroutine = null;
//        }
//    }
//}