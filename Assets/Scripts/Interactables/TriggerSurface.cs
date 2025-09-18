using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class TriggerSurface : Interactable
{
    [SerializeField] Light flashLight;
    [SerializeField] float flashlightIntesity;
    bool isTriggered = false;

    protected override void Interact()
    {
        if (!isTriggered)
        {
            Debug.Log("Trigger event triggered from triggersurface");
            //Trigger event
            flashLight.intensity = flashlightIntesity;
            isTriggered = true;

            InteractionEvent ie = GetComponent<InteractionEvent>();
            if (ie != null)
            {
                ie.OnInteract.Invoke();
            }
        }

    }
}
