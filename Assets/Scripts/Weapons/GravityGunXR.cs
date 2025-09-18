using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class GravityGunXR : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData; // Tilknyt ScriptableObject med v�rdier
    public Transform holdPoint; // Hvor objektet skal holdes
    public Transform endOfGun; // Enden af gunnen, hvor affyring sker
    public float releaseDistance = 2f;
    private Rigidbody grabbedObject;

    private InputAction grabAction;
    private InputAction attractAction;
    private InputAction throwAction;

    private InputActionMap gravityGunActionMap;

    private bool isGunGrabbed = false;
    private bool canShoot = false;

    private XRNode controllerNode = XRNode.RightHand; // Eller LeftHand, afh�ngigt af hvilken controller du bruger

    void Awake()
    {
        // Opret action map til gravity gun
        gravityGunActionMap = new InputActionMap("GravityGunActions");

        // Definer handlinger for grab, attract og throw (triggeren p� controlleren)
        grabAction = gravityGunActionMap.AddAction("Grab", binding: "<XRController>/trigger");
        attractAction = gravityGunActionMap.AddAction("Attract", binding: "<XRController>/trigger");
        throwAction = gravityGunActionMap.AddAction("Throw", binding: "<XRController>/secondaryButton");

        grabAction.performed += ctx => OnGrabPerformed(ctx);
        attractAction.performed += ctx => OnAttractPerformed(ctx);
        throwAction.performed += ctx => OnThrowPerformed(ctx);
    }

    void OnEnable()
    {
        gravityGunActionMap.Enable(); // Aktiverer action map'et, n�r det er n�dvendigt
    }

    void OnDisable()
    {
        gravityGunActionMap.Disable(); // Deaktiverer action map'et, n�r det ikke er n�dvendigt
    }

    public void OnGunGrabbed()
    {
        // N�r gunnen gribes, aktiver input til gravity gun
        Debug.Log("Gun is grabbed");
        isGunGrabbed = true;
        gravityGunActionMap.Enable(); // Aktiverer gravity gun input
    }

    public void OnGunReleased()
    {
        // N�r gunnen slippes, deaktiver input til gravity gun
        Debug.Log("Gun is released");
        isGunGrabbed = false;
        gravityGunActionMap.Disable(); // Deaktiverer gravity gun input
    }

    void Update()
    {
        if (isGunGrabbed)
        {
            if (grabAction.ReadValue<float>() > 0) // Hvis triggeren holdes nede
            {
                Debug.Log("Trigger is being held");
                if (grabbedObject == null)
                    TryGrabObject();
                else
                    AttractObject();
            }
            else if (grabAction.ReadValue<float>() == 0 && grabbedObject != null)
            {
                ReleaseObject();
                Debug.Log("Releasing object");
            }

            if (throwAction.triggered && grabbedObject != null) // H�jre trigger (sekund�r knap) for at kaste objektet
            {
                ThrowObject();
                Debug.Log("Throwing objekt");
            }
        }
    }

    void TryGrabObject()
    {
        // Raycast fra slutningen af gunnen for at fange objekter
        Ray ray = new Ray(endOfGun.position, endOfGun.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {
            Debug.Log("Gravtiy gun hit: " + hit.collider.name);
            if (hit.rigidbody != null)
            {
                grabbedObject = hit.rigidbody;
                grabbedObject.useGravity = false;
                grabbedObject.linearVelocity = Vector3.zero;
                grabbedObject.angularVelocity = Vector3.zero;
                grabbedObject.linearDamping = 10;
            }
        }
    }

    void AttractObject()
    {
        if (grabbedObject == null) return;

        Vector3 direction = (holdPoint.position - grabbedObject.position);
        float distance = direction.magnitude;

        // Objektet skal v�re t�ttere p� holdPoint f�r det kan skyde
        if (distance > releaseDistance)
        {
            grabbedObject.AddForce(direction.normalized * weaponData.attractionForce, ForceMode.Force);
            canShoot = false;
        }
        else
        {
            grabbedObject.linearVelocity = Vector3.zero;
            canShoot = true;
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 1;
            grabbedObject = null;
        }
    }

    void ThrowObject()
    {
        if (grabbedObject != null && canShoot)
        {
            Vector3 throwDirection = (endOfGun.position - holdPoint.position).normalized;
            grabbedObject.AddForce(throwDirection * weaponData.throwForce, ForceMode.Impulse);
            grabbedObject = null;
        }
    }

    // Definerer metoder for input-handling
    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        if (grabbedObject == null)
            TryGrabObject();
        else
            AttractObject();
    }

    private void OnAttractPerformed(InputAction.CallbackContext context)
    {
        if (grabbedObject != null && canShoot)
            AttractObject();
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        if (grabbedObject != null && canShoot)
            ThrowObject();
    }
}
