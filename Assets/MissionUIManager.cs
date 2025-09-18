using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI objectiveText; // Hele objectives-listen
    [SerializeField] private MissionData currentMission;

    private List<Objective> runtimeObjectives;

    public void SetObjectives(List<Objective> objectives)
    {
        runtimeObjectives = objectives;
        UpdateAllObjectivesUI();
    }

    public void UpdateAllObjectivesUI()
    {
        if (runtimeObjectives == null || runtimeObjectives.Count == 0)
        {
            objectiveText.text = "No objectives.";
            return;
        }

        objectiveText.text = "Objectives:\n";
        foreach (var objective in runtimeObjectives)
        {
            objectiveText.text += $"{objective.GetObjectiveDescription()}\n";
        }
    }


    // Opdaterer UI når noget i inventory ændrer sig
    public void UpdateUIForObjective(Objective updatedObjective)
    {
        UpdateAllObjectivesUI(); // Opdaterer hele listen af objectives
    }
}

