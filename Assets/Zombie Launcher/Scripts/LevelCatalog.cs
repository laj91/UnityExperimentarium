using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Catalog", fileName = "LevelCatalog")]
public class LevelCatalog : ScriptableObject
{
    public List<LevelData> levels = new();

    public LevelData GetById(string id) => levels.Find(l => l.levelId == id);
    public LevelData GetByIndex(int index) => (index >= 0 && index < levels.Count) ? levels[index] : null;
}