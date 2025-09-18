using UnityEngine;
using UnityEngine.InputSystem;

public class CraneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform craneGun;
    [SerializeField] private Transform platform;
    [SerializeField] private Transform playerRig;               // Skal pege på samme XR Origin som DroneSwitcher.xrRigRoot
    [SerializeField] private DroneSwitcher droneSwitcher;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference leftStickAction;

    [Header("Platform Rotation (Yaw)")]
    [SerializeField] private float platformRotationSpeed = 45f;
    [SerializeField] private float platformRotationLimit = 180f;
    [SerializeField] private AnimationCurve yawResponse = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Gun Pitch (Optional)")]
    [SerializeField] private float gunPitchSpeed = 45f;
    [SerializeField] private float gunPitchMin = -20f;
    [SerializeField] private float gunPitchMax = 60f;
    [SerializeField] private Transform gunAttachPosition;

    [Header("Attach Player")]
    [SerializeField] private bool autoAttachPlayer = true;
    [SerializeField] private bool maintainRigWorldPosition = true;
    [SerializeField] private Transform playerAttachPosition;

    [Header("Deadzone")]
    [SerializeField] private float stickDeadzone = 0.1f;

    [Header("Axis Override")]
    [SerializeField] private Axis yawAxis = Axis.Y;   // Vælg X/Y/Z for yaw
    [SerializeField] private Axis pitchAxis = Axis.X; // Vælg X/Y/Z for pitch

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool debugFrameSpam = false;
    [SerializeField] private float debugChangeThreshold = 0.02f;

    private float _startYaw;
    private float _relativeYaw;
    private float _gunPitchStart;
    private float _gunPitchOffset;

    private Transform _originalPlayerParent;
    private bool _attached;
    private bool _controlsEnabled;

    private Vector2 _lastStickLogged;
    private float _lastLoggedYaw;
    private float _lastLoggedPitch;

    // Gem startrotationer
    private Quaternion _platformStartLocalRotation;
    private Quaternion _gunStartLocalRotation;

    // Pivot-rotation for gun: roter omkring gunAttachPosition
    private Transform _gunParent;
    private Vector3 _gunOffsetToAttachLocal;  // offset fra pivot til gun i parent-rum ved start
    private bool _useAttachPivot;

    private void OnEnable()
    {
        if (droneSwitcher == null) droneSwitcher = FindObjectOfType<DroneSwitcher>();

        // Autowire XR Origin hvis ikke sat (for at sikre samme transform som DroneSwitcher flytter)
        if (playerRig == null && droneSwitcher != null)
            playerRig = droneSwitcher.transform;

        if (droneSwitcher != null)
        {
            droneSwitcher.DroneModeChanged += OnDroneModeChanged;

            if (playerRig == null)
                LogWarn("playerRig er ikke sat. Sæt den til XR Origin root (samme som DroneSwitcher.xrRigRoot).");

            OnDroneModeChanged(droneSwitcher.IsDroneActive); // init
        }
        else
        {
            _controlsEnabled = true; // antag player-mode
        }
    }

    private void OnDisable()
    {
        if (droneSwitcher == null) droneSwitcher = FindObjectOfType<DroneSwitcher>();
        if (droneSwitcher != null) droneSwitcher.DroneModeChanged -= OnDroneModeChanged;
    }

    private void Start()
    {
        if (platform == null)
            platform = transform;

        _platformStartLocalRotation = platform.localRotation;

        _startYaw = platform.localEulerAngles.y;
        if (_startYaw > 180f) _startYaw -= 360f;

        if (craneGun != null)
        {
            _gunStartLocalRotation = craneGun.localRotation;
            _gunPitchStart = 0f; // Behandler pitch-grænser relativt til startrotation
            _useAttachPivot = gunAttachPosition != null;

            if (_useAttachPivot)
            {
                _gunParent = craneGun.parent;

                if (_gunParent != null)
                {
                    // Offset i parent-rum: (gunLocal - pivotLocal)
                    Vector3 pivotLocal = _gunParent.InverseTransformPoint(gunAttachPosition.position);
                    Vector3 gunLocal = _gunParent.InverseTransformPoint(craneGun.position);
                    _gunOffsetToAttachLocal = gunLocal - pivotLocal;
                }
                else
                {
                    // Ingen parent: brug world-rum som "parent-rum"
                    _gunOffsetToAttachLocal = craneGun.position - gunAttachPosition.position;
                }

                if (gunAttachPosition.IsChildOf(craneGun))
                {
                    LogWarn("gunAttachPosition er et barn af gun. Det flytter med pistolen og kan bryde pivot-rotation. Overvej at gøre den til søskende under samme parent.");
                }
            }

            Log("Gun start rotation gemt for relativ pitch.");
        }
        else
        {
            LogWarn("Ingen craneGun sat.");
        }

        Log($"Crane init | StartYaw={_startYaw:0.00} Limit=±{platformRotationLimit} speed={platformRotationSpeed}");
    }

    private void Update()
    {
        if (!_controlsEnabled)
        {
            // Fail-safe: hvis vi er i drone-mode men stadig parentet, så detach
            if (_attached) DetachPlayer();
            return;
        }

        Vector2 stick = ReadStick();
        HandleYaw(stick.x);
        HandlePitch(stick.y);
        // NYT: send bevægelsesmagnitude til AudioManager
        AudioEvents.RaiseCraneMove(stick.magnitude);
    }

    private void OnDroneModeChanged(bool droneActive)
    {
        _controlsEnabled = !droneActive;

        if (!autoAttachPlayer)
            return;

        if (droneActive)
            DetachPlayer(); // detach i drone-mode
        else
            AttachPlayer(); // attach i player-mode
    }

    private Vector2 ReadStick()
    {
        if (leftStickAction == null || leftStickAction.action == null)
            return Vector2.zero;

        Vector2 raw = leftStickAction.action.ReadValue<Vector2>();

        if (raw.magnitude < stickDeadzone)
        {
            MaybeLogStick(raw, "Stick (deadzone)");
            return Vector2.zero;
        }

        float mag = raw.magnitude;
        float scaledMag = yawResponse.Evaluate(Mathf.Clamp01(mag));
        Vector2 processed = (mag > 0.0001f) ? raw.normalized * scaledMag : Vector2.zero;
        processed = Vector2.ClampMagnitude(processed, 1f);

        MaybeLogStick(processed, "Stick");
        return processed;
    }

    private void HandleYaw(float stickX)
    {
        if (Mathf.Approximately(stickX, 0f))
            return;

        _relativeYaw += stickX * platformRotationSpeed * Time.deltaTime;
        _relativeYaw = Mathf.Clamp(_relativeYaw, -platformRotationLimit, platformRotationLimit);

        // Roter omkring valgt akse (X/Y/Z) relativt til startrotation
        Vector3 axisParentSpace = GetYawAxisInParentSpace();
        Quaternion yawRot = Quaternion.AngleAxis(_relativeYaw, axisParentSpace);
        platform.localRotation = yawRot * _platformStartLocalRotation;

        float finalYawForLog = _startYaw + _relativeYaw;
        MaybeLogYaw(finalYawForLog, stickX);
    }

    private void HandlePitch(float stickY)
    {
        if (craneGun == null || Mathf.Approximately(stickY, 0f))
            return;

        _gunPitchOffset += stickY * gunPitchSpeed * Time.deltaTime;
        _gunPitchOffset = Mathf.Clamp(_gunPitchOffset, gunPitchMin, gunPitchMax);

        // Roter omkring valgt akse (X/Y/Z) relativt til startrotation
        Vector3 axisParentSpace = GetPitchAxisInParentSpace();
        Quaternion pitchRot = Quaternion.AngleAxis(_gunPitchOffset, axisParentSpace);

        // Sæt rotation
        craneGun.localRotation = pitchRot * _gunStartLocalRotation;

        // Hvis der er et separat pivot (gunAttachPosition), så opdater også position,
        // så pivot-punktet forbliver på gunAttachPosition.
        if (_useAttachPivot)
        {
            if (_gunParent != null)
            {
                Vector3 pivotLocalNow = _gunParent.InverseTransformPoint(gunAttachPosition.position);
                Vector3 rotatedOffsetLocal = pitchRot * _gunOffsetToAttachLocal;
                craneGun.localPosition = pivotLocalNow + rotatedOffsetLocal;
            }
            else
            {
                Vector3 pivotWorldNow = gunAttachPosition.position;
                Vector3 rotatedOffsetWorld = pitchRot * _gunOffsetToAttachLocal;
                craneGun.position = pivotWorldNow + rotatedOffsetWorld;
            }
        }

        float finalPitchForLog = _gunPitchOffset;
        MaybeLogPitch(finalPitchForLog, stickY);
    }

    public void AttachPlayer()
    {
        if (playerRig == null)
        {
            LogWarn("Kan ikke attach player: playerRig mangler.");
            return;
        }
        if (_attached) return;

        _originalPlayerParent = playerRig.parent;
        if (maintainRigWorldPosition)
        {
            playerRig.SetParent(platform, true);   // behold world pos
            playerRig.SetPositionAndRotation(platform.position, platform.rotation);
        }
        else
        {
            playerRig.SetParent(playerAttachPosition, false); // snap lokalt
        }

        _attached = true;
        Log("Player attached til platform.");
    }

    public void DetachPlayer()
    {
        if (!_attached || playerRig == null) return;

        if (_originalPlayerParent != null)
            playerRig.SetParent(_originalPlayerParent, true);
        else
            playerRig.SetParent(null, true);

        _attached = false;
        Log("Player detached fra platform.");
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }

    private enum Axis { X, Y, Z }

    private static Vector3 AxisToVector(Axis a)
    {
        switch (a)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default: return Vector3.forward; // Z
        }
    }

    // Konverter valgt lokal akse til parent-rum (til venstre-multiplikation: AxisRot * StartRot)
    private Vector3 GetYawAxisInParentSpace()
    {
        Vector3 localAxis = AxisToVector(yawAxis);
        return (_platformStartLocalRotation * localAxis).normalized;
    }

    private Vector3 GetPitchAxisInParentSpace()
    {
        Vector3 localAxis = AxisToVector(pitchAxis);
        return (_gunStartLocalRotation * localAxis).normalized;
    }

    #region Debug Helpers
    private void MaybeLogStick(Vector2 v, string label)
    {
        if (!debugLogs) return;
        if (debugFrameSpam)
        {
            Log($"{label} raw={v} mag={v.magnitude:0.00}");
            return;
        }
        if ((v - _lastStickLogged).sqrMagnitude >= debugChangeThreshold * debugChangeThreshold)
        {
            _lastStickLogged = v;
            Log($"{label}={v} mag={v.magnitude:0.00}");
        }
    }

    private void MaybeLogYaw(float finalYaw, float inputX)
    {
        if (!debugLogs) return;
        if (debugFrameSpam)
        {
            Log($"Yaw input={inputX:0.00} rel={_relativeYaw:0.00} final={finalYaw:0.00}");
            return;
        }
        if (Mathf.Abs(finalYaw - _lastLoggedYaw) >= platformRotationSpeed * Time.deltaTime * 0.5f)
        {
            _lastLoggedYaw = finalYaw;
            Log($"Yaw -> input={inputX:0.00} rel={_relativeYaw:0.0} final={finalYaw:0.0}");
        }
    }

    private void MaybeLogPitch(float finalPitch, float inputY)
    {
        if (!debugLogs) return;
        if (debugFrameSpam)
        {
            Log($"Pitch input={inputY:0.00} offset={_gunPitchOffset:0.00} final={finalPitch:0.00}");
            return;
        }
        if (Mathf.Abs(finalPitch - _lastLoggedPitch) >= gunPitchSpeed * Time.deltaTime * 0.5f)
        {
            _lastLoggedPitch = finalPitch;
            Log($"Pitch -> input={inputY:0.00} offset={_gunPitchOffset:0.0} final={finalPitch:0.0}");
        }
    }

    private void Log(string msg)
    {
        if (!debugLogs) return;
        Debug.Log($"[Crane] {msg}", this);
    }

    private void LogWarn(string msg)
    {
        if (!debugLogs) return;
        Debug.LogWarning($"[Crane] {msg}", this);
    }
    #endregion
}
