using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI resultText; // (valgfrit) drag et UI Text til resultat

    [Header("Level References")]
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private LevelData currentLevel;

    [Header("Runtime State (Visible)")]
    [SerializeField] private int score = 0;
    [SerializeField] private int objectsHit = 0;
    [SerializeField] private int goalsHit = 0;
    [SerializeField] private int shotsFired = 0;

    [Header("Events")]
    [SerializeField] private UnityEvent goalReachedEvent = new UnityEvent();   // Success
    [SerializeField] private UnityEvent levelFailedEvent = new UnityEvent();   // Failure
    public UnityEvent GoalReachedEvent => goalReachedEvent;
    public UnityEvent LevelFailedEvent => levelFailedEvent;
    public bool LevelActive => levelActive;

    private bool levelActive = false;
    private bool levelEnded = false;
    private float startTime;

    // Public read-only
    public LevelData CurrentLevel => currentLevel;
    public int Score => score;
    public int ObjectsHit => objectsHit;
    public int GoalsHit => goalsHit;
    public int ShotsFired => shotsFired;
    public bool LevelEnded => levelEnded;

    private void Start()
    {
        if (currentLevel == null && levelCatalog != null && levelCatalog.levels.Count > 0)
            currentLevel = levelCatalog.levels[0];

        BeginLevel();
    }

    public void BeginLevel()
    {
        if (currentLevel == null)
        {
            Debug.LogError("No currentLevel assigned.");
            return;
        }

        score = 0;
        objectsHit = 0;
        goalsHit = 0;
        shotsFired = 0;
        levelEnded = false;
        levelActive = true;
        startTime = Time.time;

        if (levelName) levelName.text = "Level:" + currentLevel.displayName;
        if (resultText) resultText.text = "";
        UpdateScoreUI();
        UpdateTimerUI(); // initial visning

        Debug.Log($"Level started: {currentLevel.displayName}");
    }

    // Kald fra Launcher fr et nyt skud affyres.
    // Returnerer true hvis skuddet m affyres/spilles; false hvis level er slut eller ikke aktiv.
    public bool TryConsumeShot()
    {
        if (!levelActive || levelEnded) return false;

        // Tjek om vi allerede har brugt alle skud
        if (shotsFired >= currentLevel.numberOfRagdollsBullets)
        {
            // Alle skud er brugt, afslut level hvis målene ikke er nået
            if (goalsHit < currentLevel.numberOfGoalsToHit)
            {
                EndLevel(false);
            }
            return false; // Stop launcher i at spawne ragdoll
        }

        // Vi har stadig skud tilbage, så øg tælleren og tillad skuddet
        shotsFired++;
        UpdateScoreUI();
        return true;
    }

    // Almindeligt objekt der giver pointsPerObject men ikke tller mod gennemfrsel.
    public void RegisterObjectHit()
    {
        if (!levelActive || levelEnded) return;

        objectsHit++;
        score += currentLevel.pointsPerObject;
        UpdateScoreUI();
    }

    // Ml der bde giver pointsPerGoal og tller mod gennemfrsel.
    public void RegisterGoalHit()
    {
        if (!levelActive || levelEnded) return;

        goalsHit++;
        score += currentLevel.pointsPerGoal;
        UpdateScoreUI();

        if (goalsHit >= currentLevel.numberOfGoalsToHit)
        {
            EndLevel(success: true);
        }
        else
        {
            // Hvis vi har brugt alle skud og stadig ikke frdig - fail
            if (shotsFired >= currentLevel.numberOfRagdollsBullets)
            {
                EndLevel(false);
            }
        }
    }

    private void Update()
    {
        if (!levelActive || levelEnded) return;

        if (currentLevel.timeLimitSeconds > 0f)
        {
            float elapsed = Time.time - startTime;
            float remaining = Mathf.Max(0f, currentLevel.timeLimitSeconds - elapsed);
            UpdateTimerUI(remaining);

            if (elapsed >= currentLevel.timeLimitSeconds)
            {
                EndLevel(success: false);
            }
        }
    }

    private void EndLevel(bool success)
    {
        if (levelEnded) return;
        levelEnded = true;
        levelActive = false;

        int timeBonus = 0;
        if (success && currentLevel.timeLimitSeconds > 0f)
        {
            float elapsed = Time.time - startTime;
            float remaining = Mathf.Max(0f, currentLevel.timeLimitSeconds - elapsed);
            timeBonus = Mathf.CeilToInt(remaining) * currentLevel.timeBonusPerSecond;
            score += timeBonus;
        }

        if (success)
        {
            score += currentLevel.bonusOnCompletion;
            UpdateScoreUI();
            goalReachedEvent?.Invoke();
            if (resultText)
                resultText.text = $"SUCCESS!\nScore: {score}\nGoals: {goalsHit}/{currentLevel.numberOfGoalsToHit}\nTimeBonus: {timeBonus}";
            Debug.Log($"Level success. Final score: {score}");
        }
        else
        {
            UpdateScoreUI();
            levelFailedEvent?.Invoke();
            if (resultText)
                resultText.text = $"FAILED\nScore: {score}\nGoals: {goalsHit}/{currentLevel.numberOfGoalsToHit}\nShots: {shotsFired}/{currentLevel.numberOfRagdollsBullets}";
            Debug.Log($"Level failed. Score: {score}");
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText)
            scoreText.text = $"Score: {score}  (Obj:{objectsHit}  Goals:{goalsHit}/{currentLevel.numberOfGoalsToHit}  Shots:{shotsFired}/{currentLevel.numberOfRagdollsBullets})";
    }

    private void UpdateTimerUI(float? remainingOverride = null)
    {
        if (!timerText) return;

        if (currentLevel.timeLimitSeconds <= 0f)
        {
            timerText.text = "";
            return;
        }

        float remaining = remainingOverride ?? Mathf.Max(0f, currentLevel.timeLimitSeconds - (Time.time - startTime));
        timerText.text = $"Time: {remaining:F1}s";
    }

    // UI hook til “Prv igen”
    public void RetryLevel()
    {
        if (currentLevel == null) return;

        if (!string.IsNullOrEmpty(currentLevel.sceneName))
        {
            SceneManager.LoadScene(currentLevel.sceneName);
        }
        else
        {
            BeginLevel();
        }
    }

    // UI hook til “Nste bane”
    public void LoadNextLevel()
    {
        if (levelCatalog == null || currentLevel == null) return;
        int index = levelCatalog.levels.IndexOf(currentLevel);
        if (index < 0) return;
        int next = index + 1;
        if (next >= levelCatalog.levels.Count)
        {
            Debug.Log("No more levels.");
            return;
        }
        LoadLevel(levelCatalog.levels[next]);
    }

    public void LoadLevelByIndex(int index)
    {
        var next = levelCatalog?.GetByIndex(index);
        if (next == null)
        {
            Debug.LogError($"No level at index {index}");
            return;
        }
        LoadLevel(next);
    }

    public void LoadLevel(LevelData levelData)
    {
        currentLevel = levelData;
        if (!string.IsNullOrEmpty(levelData.sceneName))
        {
            SceneManager.LoadScene(levelData.sceneName);
        }
        else
        {
            BeginLevel();
        }
    }
}
