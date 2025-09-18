using UnityEngine;
using UnityEngine.InputSystem;

public class MockController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _maximumForce = 10f;
    [SerializeField] private float _maximumForceTime = 1f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference fireAction; // Bind to Mock/Fire (Space)

    private float _timeFireStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Intentionally empty when using new Input System callbacks
    }

    private void OnEnable()
    {
        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.started += OnFireStarted;
            fireAction.action.canceled += OnFireReleased;
            if (!fireAction.action.enabled) fireAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.started -= OnFireStarted;
            fireAction.action.canceled -= OnFireReleased;
        }
    }

    private void OnFireStarted(InputAction.CallbackContext ctx)
    {
        _timeFireStarted = Time.time;
    }

    private void OnFireReleased(InputAction.CallbackContext ctx)
    {
        Camera cam = _camera != null ? _camera : Camera.main;
        if (cam == null) return;

        Vector2 mousePos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Zombie zombie = hitInfo.collider.GetComponentInParent<Zombie>();
            if (zombie != null)
            {
                float hold = Time.time - _timeFireStarted;
                float forcePct = Mathf.Clamp01(hold / _maximumForceTime);
                float forceMag = Mathf.Lerp(1, _maximumForce, forcePct);

                Vector3 dir = (zombie.transform.position - cam.transform.position);
                dir.y = 1f;
                dir.Normalize();

                Vector3 force = forceMag * dir;
                zombie.TriggerRagdoll(force, hitInfo.point);
            }
        }
    }
}
