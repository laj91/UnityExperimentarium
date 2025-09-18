using StarterAssets;
using UnityEngine;

public class FixItem : MonoBehaviour
{
    private StarterAssetsInputs starterAssetsInputs;

    private void Start()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }
    private void Update()
    {
        if (starterAssetsInputs.fixItem) // Venstre klik
        {
            Debug.Log("Trykket p interact");
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 3f))
            {
                BrokenItem toilet = hit.collider.GetComponent<BrokenItem>();
                if (toilet != null)
                {
                    toilet.Repair();
                }
            }



        }
        starterAssetsInputs.fixItem = false;
    }
}
