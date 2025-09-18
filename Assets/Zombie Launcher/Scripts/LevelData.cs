using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Data", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identification")]
    public string levelId; // Unique key
    public string displayName;
    public string sceneName; // Must match scene in Build Settings

    [Header("Objectives")]
    public float timeLimitSeconds = 0f; // 0 = ingen tidsbegr�nsning
    public int numberOfRagdollsBullets = 5; // Antal skud / fors�g (kan bruges senere)
    public int numberOfGoalsToHit = 3; // Krav for succes
    public int currentGoalIndex = 0;  // Hvis du vil have sekventielle m�l (valgfrit)

    [Header("Scoring")]
    public int pointsPerObject = 10; // Almindeligt objekt
    public int pointsPerGoal = 50;  // M�l der t�ller mod gennemf�rsel
    public int timeBonusPerSecond = 5; // Bonus pr. helt sekund tilbage (kun ved succes)
    public int bonusOnCompletion = 0; // Fast bonus ved succes

    [Header("Presentation")]
    public Sprite previewImage;

    [Header("Tuning Parameters")]
    public AnimationCurve difficultyOverTime = AnimationCurve.Linear(0, 1, 1, 1);
}