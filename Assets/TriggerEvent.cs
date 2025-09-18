using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        Debug.Log("Toilettet er i stykker! Sluk vandet og find svupperen for at reparerererere det!");
    }
}
