using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MountedWeaponController : XRGrabInteractable
{
    [Header("Pivot Ops�tning")]
    [Tooltip("Det objekt, som v�bnet roterer omkring.")]
    public Transform gunMountPivot;

    [Header("Rotation")]
    [Tooltip("Hastighed for glidende rotation mod m�lretning.")]
    public float rotationSpeed = 5f;

    // Vi gemmer interactor reference til de h�nder, som griber v�bnet.
    private XRBaseInteractor primaryInteractor = null;
    private XRBaseInteractor secondaryInteractor = null;

    // N�r en h�nd griber v�bnet, registreres den
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // Hvis ingen h�nd er registreret, s�t som prim�r
        if (primaryInteractor == null)
        {
            primaryInteractor = args.interactorObject as XRBaseInteractor;
        }
        // Hvis der allerede er en prim�r, s�t den nye som sekund�r
        else if (secondaryInteractor == null)
        {
            secondaryInteractor = args.interactorObject as XRBaseInteractor;
        }
    }

    // N�r en h�nd slipper v�bnet, rydder vi referencen
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;
        if (interactor == secondaryInteractor)
        {
            secondaryInteractor = null;
        }
        else if (interactor == primaryInteractor)
        {
            // Hvis prim�r frigives, flyt eventuel sekund�r til prim�rposition
            primaryInteractor = secondaryInteractor;
            secondaryInteractor = null;
        }
    }

    // Denne funktion kaldes, n�r spilleren trykker p� triggeren (eller andet aktiveringsinput)
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        FireWeapon();
    }

    // Skyd-funktionen � her kan du fx instantiere kugler, afspille effekter osv.
    private void FireWeapon()
    {
        Debug.Log("FIRE!");  // For prototyping: log skud til konsollen
        // Her kan du senere tilf�je projektil-instansiering eller raycast logik.
    }

    private void Update()
    {
        // Kun opdater rotation hvis begge h�nder griber
        if (primaryInteractor != null && secondaryInteractor != null)
        {
            Vector3 primaryPos = primaryInteractor.transform.position;
            Vector3 secondaryPos = secondaryInteractor.transform.position;
            Vector3 direction = secondaryPos - primaryPos;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                gunMountPivot.rotation = Quaternion.Slerp(gunMountPivot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}