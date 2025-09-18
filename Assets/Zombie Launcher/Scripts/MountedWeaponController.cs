using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MountedWeaponController : XRGrabInteractable
{
    [Header("Pivot Opsætning")]
    [Tooltip("Det objekt, som våbnet roterer omkring.")]
    public Transform gunMountPivot;

    [Header("Rotation")]
    [Tooltip("Hastighed for glidende rotation mod målretning.")]
    public float rotationSpeed = 5f;

    // Vi gemmer interactor reference til de hænder, som griber våbnet.
    private XRBaseInteractor primaryInteractor = null;
    private XRBaseInteractor secondaryInteractor = null;

    // Når en hånd griber våbnet, registreres den
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // Hvis ingen hånd er registreret, sæt som primær
        if (primaryInteractor == null)
        {
            primaryInteractor = args.interactorObject as XRBaseInteractor;
        }
        // Hvis der allerede er en primær, sæt den nye som sekundær
        else if (secondaryInteractor == null)
        {
            secondaryInteractor = args.interactorObject as XRBaseInteractor;
        }
    }

    // Når en hånd slipper våbnet, rydder vi referencen
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
            // Hvis primær frigives, flyt eventuel sekundær til primærposition
            primaryInteractor = secondaryInteractor;
            secondaryInteractor = null;
        }
    }

    // Denne funktion kaldes, når spilleren trykker på triggeren (eller andet aktiveringsinput)
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        FireWeapon();
    }

    // Skyd-funktionen – her kan du fx instantiere kugler, afspille effekter osv.
    private void FireWeapon()
    {
        Debug.Log("FIRE!");  // For prototyping: log skud til konsollen
        // Her kan du senere tilføje projektil-instansiering eller raycast logik.
    }

    private void Update()
    {
        // Kun opdater rotation hvis begge hænder griber
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