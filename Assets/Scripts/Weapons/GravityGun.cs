using UnityEngine;

public class GravityGun : WeaponBase
{
    [SerializeField] private WeaponData weaponData; // Tilknyt ScriptableObject med værdier
    public float releaseDistance = 2f;
    public Transform holdPoint;
    public Camera cam;
    private Rigidbody grabbedObject;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbedObject == null)
                TryGrabObject();
            else
                AttractObject();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }

        if (Input.GetMouseButtonDown(1) && grabbedObject != null)
        {
            ThrowObject();
        }
    }
    public override void Use()
    {
        // Brug GravityGun (venstre klik)
        if (grabbedObject == null) TryGrabObject();
        else AttractObject();
    }
    void TryGrabObject()
    {
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {
            if (hit.rigidbody != null)
            {
                grabbedObject = hit.rigidbody;
                grabbedObject.useGravity = false;
                grabbedObject.linearDamping = 10;
            }
        }
    }

    void AttractObject()
    {
        Vector3 direction = (holdPoint.position - grabbedObject.position); // Retning mod gun
        float distance = direction.magnitude;

        // Hvis objektet er t�ttere end releaseDistance, stopper vi tiltr�kningen
        if (distance > releaseDistance)
        {
            // Beregn og p�f�r en kraft, der tr�kker objektet mod gravity gun
            grabbedObject.AddForce(direction.normalized * weaponData.attractionForce, ForceMode.Force);
        }
        else
        {
            // Stop tiltr�kningen, n�r objektet er t�t nok p�
            grabbedObject.linearVelocity = Vector3.zero;
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
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject.linearDamping = 1;
            grabbedObject.AddForce(cam.transform.forward * weaponData.throwForce, ForceMode.Impulse);
            grabbedObject = null;
        }
    }
   

}

