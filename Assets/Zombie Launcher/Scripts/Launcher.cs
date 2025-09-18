using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;   // Drag & drop (auto-find hvis null i Start)
    [SerializeField] GameObject ragdoll;
    [SerializeField] Transform launchPosition;
    [SerializeField] float launchForce = 10f;
    [SerializeField] float launchTorque = 10f;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Vector3 cameraOffset = new Vector3(0, 1, -5);
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] InputActionReference triggerAction;
    [SerializeField] float minLaunchForce = 5f;
    [SerializeField] float maxLaunchForce = 30f;
    [SerializeField] float maxHoldTime = 2f;

    [Header("Trajectory Preview")]
    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] int trajectoryPoints = 30;
    [SerializeField] float timeBetweenPoints = 0.1f;
    [SerializeField] LayerMask collisionMask = ~0;
    [SerializeField] bool autoConfigureLine = true;

    public void LockShootingOn() => LockShooting(true);
    public void LockShootingOff() => LockShooting(false);   

    private float currentSliderValue = 0f;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isTriggerHeld = false;
    private float triggerHoldTime = 0f;
    private bool isShootingLocked = false;
    private bool _chargeMaxedSent = false; // NYT
    private void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        Cursor.lockState = CursorLockMode.Locked;

        if (trajectoryLine != null && autoConfigureLine)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.widthMultiplier = 0.05f;
        }
    }

    private void Update()
    {
        if (isTriggerHeld)
        {
            triggerHoldTime += Time.deltaTime;
            triggerHoldTime = Mathf.Min(triggerHoldTime, maxHoldTime);

            float t = triggerHoldTime / maxHoldTime;
            float previewForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, t);

            // NYT: meld opladningsprogress
            AudioEvents.RaiseLaunchChargeProgress(t);

            // NYT: stop opladningslyd når maks er nået (én gang)
            if (t >= 1f && !_chargeMaxedSent)
            {
                _chargeMaxedSent = true;
                AudioEvents.RaiseLaunchChargeMaxed();
            }
            UpdateTrajectory(previewForce);
        }
        else
        {
            if (trajectoryLine != null && trajectoryLine.positionCount > 0)
                ClearTrajectory();
        }
    }

    public void LockShooting(bool lockShooting)
    {
        isShootingLocked = lockShooting;
        if (isShootingLocked)
        {
            ClearTrajectory();
        }
    }
    //dsdfsdfss
    //asdsdasdssss
    //ffsss
    private void OnEnable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.started += OnTriggerStarted;
            triggerAction.action.canceled += OnTriggerReleased;
        }
    }

    private void OnDisable()
    {
        if (triggerAction != null)
        {
            triggerAction.action.started -= OnTriggerStarted;
            triggerAction.action.canceled -= OnTriggerReleased;
        }
    }

    private void OnTriggerStarted(InputAction.CallbackContext context)
    {
        if (isShootingLocked) return;
        isTriggerHeld = true;
        triggerHoldTime = 0f;
        // NYT: opladning starter
        AudioEvents.RaiseLaunchChargeStarted();
    }

    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        if (isShootingLocked) return;

        isTriggerHeld = false;
        float t = triggerHoldTime / maxHoldTime;
        float force = Mathf.Lerp(minLaunchForce, maxLaunchForce, t);
        ClearTrajectory();
        // NYT: opladning slut (uanset om vi affyrer eller ej)
        AudioEvents.RaiseLaunchChargeEnded();
        // Spørg GameManager om vi må bruge et skud
        if (gameManager != null)
        {
            if (!gameManager.TryConsumeShot())
            {
                // Level enten slut eller skudlimit nået ? lås
                LockShooting(true);
                return;
            }
        }

        // Hvis level ikke sluttede inde i TryConsumeShot, så spawn
        if (gameManager == null || (gameManager.LevelActive && !gameManager.LevelEnded))
        {
            LaunchZombie(force);
            // NYT: affyrings-event (afspil launcherFire)
            AudioEvents.RaiseLaunchFired(force);
            Debug.Log($"Zombie launched with force: {force}");
        }
        else
        {
            LockShooting(true);
        }
    }

    private void LaunchZombie(float force)
    {
        if (isShootingLocked) return;

        GameObject zombieInstance = Instantiate(ragdoll, launchPosition.position, launchPosition.rotation);
        Animator animator = zombieInstance.GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        Rigidbody[] allRigidbodies = zombieInstance.GetComponentsInChildren<Rigidbody>();
        Vector3 initialVelocity = launchPosition.forward * force;

        foreach (var body in allRigidbodies)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.AddForce(initialVelocity, ForceMode.VelocityChange);
            body.AddTorque(Random.onUnitSphere * launchTorque, ForceMode.VelocityChange);
        }
    }

    private void UpdateTrajectory(float force)
    {
        if (trajectoryLine == null) return;

        Vector3 startPos = launchPosition.position;
        Vector3 startVel = launchPosition.forward * force;

        if (trajectoryPoints <= 0) return;

        trajectoryLine.positionCount = 0;

        Vector3 previousPoint = startPos;
        float accumulatedTime = 0f;
        int positionsUsed = 0;

        for (int i = 1; i <= trajectoryPoints; i++)
        {
            accumulatedTime += timeBetweenPoints;
            Vector3 point = CalculatePositionAtTime(startPos, startVel, accumulatedTime);

            Vector3 segmentDir = point - previousPoint;
            float dist = segmentDir.magnitude;
            if (dist > 0f)
            {
                if (Physics.Raycast(previousPoint, segmentDir.normalized, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    AddPoint(previousPoint, positionsUsed++);
                    AddPoint(hit.point, positionsUsed++);
                    break;
                }
            }

            AddPoint(previousPoint, positionsUsed++);
            previousPoint = point;

            if (i == trajectoryPoints)
            {
                AddPoint(point, positionsUsed++);
            }
        }

        trajectoryLine.positionCount = positionsUsed;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.2f;
    }

    private Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 startVel, float time)
    {
        return startPos + startVel * time + 0.5f * Physics.gravity * (time * time);
    }

    private void AddPoint(Vector3 point, int index)
    {
        if (trajectoryLine.positionCount <= index)
        {
            trajectoryLine.positionCount = index + 1;
        }
        trajectoryLine.SetPosition(index, point);
    }

    private void ClearTrajectory()
    {
        if (trajectoryLine != null)
            trajectoryLine.positionCount = 0;
    }
}













