using UnityEngine;

public class LevelSelectionUI : MonoBehaviour
{
    [SerializeField] private LevelCatalog catalog;
    [SerializeField] private GameManager gameManager;

    public void SelectLevel(int index)
    {
        gameManager.LoadLevelByIndex(index);
    }
}