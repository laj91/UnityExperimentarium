using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
[CreateAssetMenu(fileName = "NewMission", menuName = "Mission")]
public class MissionData : ScriptableObject
{
    public string missionName;
    public string missionDescription;
    public int missionID;
    [Description("0 = Main, 1 = Side, 2 = Optional")]
    public int missionType; // 0 = Main, 1 = Side, 2 = Optional
    public int missionReward; // Reward for completing the mission
    public List<ObjectiveData> Objectives; // List of objectives for the mission
    //public List<int> enemyIDs; // Array of enemy IDs that are part of the mission
    //public List<Vector3> waypoints; // Array of waypoints for the mission
}
