using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool useEvents;
    public string promptMessage;
   
    
    public void BaseInteract()
    {
        Interact();
    }

    protected virtual void Interact()
    {
       //fgh
    }
}
