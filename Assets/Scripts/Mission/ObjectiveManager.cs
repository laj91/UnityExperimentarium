using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    private List<Objective> activeObjectives = new List<Objective>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Valgfrit, hvis du vil beholde den mellem scener
    }

    public void AddObjective(Objective objective)
    {
        if (!activeObjectives.Contains(objective))
        {
            activeObjectives.Add(objective);
            //objective.CompleteObjective(); // Evt. for at nulstille fremskridt
        }
    }

    public void RemoveObjective(Objective objective)
    {
        if (activeObjectives.Contains(objective))
        {
            activeObjectives.Remove(objective);
        }
    }

    public void TriggerObjective(IObjectiveTrigger trigger)
    {
        foreach (var objective in activeObjectives)
        {
            trigger.Trigger(objective);

            if (objective.IsCompleted)
            {
                Debug.Log($"Objective completed: {objective}");
                // Her kunne du f.eks. aktivere næste mission, give belønning osv.
            }
        }
    }

    public IReadOnlyList<Objective> GetActiveObjectives() => activeObjectives;
}
