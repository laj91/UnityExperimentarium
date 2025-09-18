using UnityEngine;

public class BrokenItem : MonoBehaviour
{
    [SerializeField] string brokenItemName;
    [SerializeField] ParticleSystem waterSpray;
    [SerializeField] bool isBroken = true;

    public void Interact()
    {
        if (isBroken)
        {
            Debug.Log($"{brokenItemName} er defekt! Vand sprøjter overalt!");

            if (waterSpray != null) 
            {
                waterSpray.Play();
            }
            
        }
        else
        {
            Debug.Log($"{brokenItemName} fungerer normalt nu.");
            if (waterSpray != null)
            {
                waterSpray.Stop();
            }
            
        }
    }

    public void Repair()
    {
        isBroken = false;
        Debug.Log($"Du har repareret {brokenItemName}!");

        if (waterSpray != null)
        waterSpray.Stop();
    }
}
