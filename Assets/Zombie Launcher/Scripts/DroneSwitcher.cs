using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
//
public class DroneSwitcher : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference switchDroneAction; // Global/Switch action
    [SerializeField] private float debounce = 0.12f;

    [Header("Rig (én XR Origin) + ankre")]
    [SerializeField] private Transform xrRigRoot;     // XR Origin root (ikke kameraet)
    [SerializeField] private Transform playerAnchor;  // Player-position/orientering
    [SerializeField] private Transform droneAnchor;   // Drone-position/orientering

    [Header("Locomotion providers (tænd/sluk pr. mode)")]
    [SerializeField] private DynamicMoveProvider moveProvider;               // Toggle: enableFly
    [SerializeField] private SnapTurnProvider snapTurnProvider;              // Toggle: enableTurnLeftRight / enableTurnAround
    [SerializeField] private ContinuousTurnProvider continuousTurnProvider;  // Toggle: enableTurnLeftRight / enableTurnAround
    [SerializeField] private bool useContinuousTurnInDrone = true;           // Ellers bruges snap turn

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public event Action<bool> DroneModeChanged;
    public bool IsDroneActive => _isDroneActive;

    [SerializeField] private bool startInDrone = false;
    private bool _isDroneActive;
    private float _lastToggle;

    private void Awake()
    {
        _isDroneActive = startInDrone;
    }

    private void OnEnable()
    {
        var action = switchDroneAction != null ? switchDroneAction.action : null;
        if (action != null)
        {
            action.performed += OnSwitchPerformed;
            if (!action.enabled) action.Enable();
        }

        ApplyMode(_isDroneActive);
        if (debugLogs) Debug.Log($"[DroneSwitcher] Init mode={(IsDroneActive ? "Drone" : "Player")}", this);
    }

    private void OnDisable()
    {
        var action = switchDroneAction != null ? switchDroneAction.action : null;
        if (action != null) action.performed -= OnSwitchPerformed;
    }

    private void OnSwitchPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - _lastToggle < debounce) return;
        _lastToggle = Time.unscaledTime;
        ToggleMode();
    }

    [ContextMenu("Toggle Drone Mode")]
    public void ToggleMode()
    {
        _isDroneActive = !_isDroneActive;
        ApplyMode(_isDroneActive);
        DroneModeChanged?.Invoke(_isDroneActive);
        if (debugLogs) Debug.Log($"[DroneSwitcher] {(IsDroneActive ? "Drone ON" : "Player ON")}", this);
    }

    private void ApplyMode(bool droneActive)
    {
        // 1) Flyt XR Origin til valgt anker
        if (xrRigRoot != null)
        {
            var anchor = droneActive ? droneAnchor : playerAnchor;
            if (anchor != null)
                xrRigRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
        }

        // 2) Toggle præcis de indstillinger du har markeret
        if (moveProvider != null)
            moveProvider.enableFly = droneActive;

        // Continuous turn aktiv i drone-mode hvis valgt
        if (continuousTurnProvider != null)
        {
            bool on = droneActive && useContinuousTurnInDrone;
            continuousTurnProvider.enableTurnLeftRight = on;
            continuousTurnProvider.enableTurnAround    = on;
        }

        // Snap turn aktiv i drone-mode hvis continuous ikke bruges
        if (snapTurnProvider != null)
        {
            bool on = droneActive && !useContinuousTurnInDrone;
            snapTurnProvider.enableTurnLeftRight = on;
            snapTurnProvider.enableTurnAround    = on;
        }

        if (debugLogs)
        {
            string rig = xrRigRoot != null ? xrRigRoot.name : "<null XR Rig>";
            string cont = continuousTurnProvider != null ? $"Cont(LR={continuousTurnProvider.enableTurnLeftRight},TA={continuousTurnProvider.enableTurnAround})" : "Cont=<null>";
            string snap = snapTurnProvider != null ? $"Snap(LR={snapTurnProvider.enableTurnLeftRight},TA={snapTurnProvider.enableTurnAround})" : "Snap=<null>";
            string move = moveProvider != null ? $"Move(Fly={moveProvider.enableFly})" : "Move=<null>";
            Debug.Log($"[DroneSwitcher] Mode={(droneActive ? "Drone" : "Player")} | XR='{rig}' | {move} | {cont} | {snap}", this);
        }
    }
}