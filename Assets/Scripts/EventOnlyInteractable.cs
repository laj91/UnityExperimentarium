using UnityEngine;

public class EventOnlyInteractable : Interactable
{
    protected override void Interact()
    {
        Debug.Log("Invoking InteractionEvent.OnInteract");
        InteractionEvent ie = GetComponent<InteractionEvent>();
        if (ie != null)
        {
            ie.OnInteract.Invoke();
        }
    }
}
