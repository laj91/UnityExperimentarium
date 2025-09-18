using StarterAssets;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] private float distance = 3f;
    [SerializeField] LayerMask mask;
    private PlayerUI playerUI;
    private StarterAssetsInputs inputs;

    private void Start()
    {
        playerUI = GetComponent<PlayerUI>();
        inputs = GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        playerUI.UpdateText(string.Empty);
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, distance, mask))
        {
            if (hit.collider.GetComponent<Interactable>() != null)
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                playerUI.UpdateText(hit.collider.GetComponent<Interactable>().promptMessage);

                if (inputs.interact)
                {
                    interactable.BaseInteract();
                    inputs.interact = false;
                }
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Interactable>() != null)
        {
            Debug.Log("Triggered interactable");
            Interactable interactable = other.GetComponent<Interactable>();
            interactable.BaseInteract();
        }
            
    }
}
