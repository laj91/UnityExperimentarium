using UnityEngine;


public abstract class ObjectiveData : ScriptableObject
{
    public string objectiveName;
    public string description;
    public int objectiveID;
    public abstract Objective CreateRuntimeObjective();

}
