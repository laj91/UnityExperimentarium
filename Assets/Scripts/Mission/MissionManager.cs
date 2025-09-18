using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public MissionData currentMission;
    public Inventory playerInventory;
    //public List<ObjectiveData> objectives;
    private List<Objective> runtimeObjectives;
    [SerializeField] private MissionUIManager uiManager;
    bool allObjectivesCompleted = true;

    void Start()
    {
        runtimeObjectives = new List<Objective>();

        foreach (var objectiveData in currentMission.Objectives)
        {
            Objective runtimeObjective;
            if (objectiveData is CollectItemObjectiveData collectItemData)
            {
                runtimeObjective = new CollectItemObjective(
                    collectItemData.itemPrefab,
                    collectItemData.requiredAmount,
                    playerInventory
                );
            }
            else
            {
                runtimeObjective = objectiveData.CreateRuntimeObjective();
            }

            runtimeObjectives.Add(runtimeObjective);
        }

        uiManager.SetObjectives(runtimeObjectives); // <-- Send alle objectives til UI
    }





    private void Update()
    {
        // Check if all objectives are completed
        

        foreach (Objective objective in runtimeObjectives)
        {
            if (!objective.IsCompleted)
            {
                Debug.Log($"All Objectives not completed: {objective}");
                allObjectivesCompleted = false;
                break;
            }

            else
            {
                allObjectivesCompleted = true;
                Debug.Log($"All Objectives completed: {objective}");
            }
        }

        if (allObjectivesCompleted)
        {
            MissionCompleted();
        }
    }

    public void OnInventoryChanged()
    {
        foreach (var obj in runtimeObjectives)
        {
            if (obj is CollectItemObjective collectObjective)
            {
                Debug.Log($"Checking progress for objective: {collectObjective}");
                collectObjective.CheckProgress(playerInventory);

                // ?? Opdater UI
                uiManager.UpdateUIForObjective(collectObjective);
            }
        }
    }


    private void MissionCompleted()
    {
        Debug.Log("Mission Completed!");
        if (runtimeObjectives.Count > 0)
        {
            foreach (var obj in runtimeObjectives)
            {
                if (!obj.IsCompleted)
                {
                    Debug.Log($"Objective not completed.");
                    return;
                }
            }
            Debug.Log("All objectives completed!");
        }
        else
        {
            Debug.Log("No objectives to complete.");
        }
    }

    private void DestroyItemInScene(int itemID)
    {
        var itemObject = FindObjectsOfType<Item>(); // Locate all items in the scene
        foreach (var item in itemObject)
        {
            if (item.itemID == itemID)
            {
                Destroy(item);
                break;
            }
        }
    }
}
    


