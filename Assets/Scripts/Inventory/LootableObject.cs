using UnityEngine;
using System.Collections.Generic;

public class LootableObject : MonoBehaviour
{
    public List<ItemStack> loot = new List<ItemStack>();
    public float interactionRange = 2f;
    private Transform playerTransform;
    private bool canLoot = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Intet GameObject med tagget 'Player' fundet i scenen.");
            enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            canLoot = distanceToPlayer <= interactionRange;

            if (canLoot)
            {
                Debug.Log("Tryk på 'F' for at åbne kisten.");
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Inventory inventory = playerTransform.GetComponent<Inventory>();
                    if (inventory != null)
                    {
                        foreach (ItemStack stack in loot)
                        {
                            //inventory.AddItem(stack.item);
                        }
                        Debug.Log("Kiste tømt.");
                        Destroy(gameObject); // Fjern kisten efter tømning
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}