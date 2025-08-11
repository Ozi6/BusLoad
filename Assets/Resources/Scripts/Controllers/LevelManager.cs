using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelManager
{
    private static LevelData _selectedLevel;

    public static void SelectLevel(LevelData levelData)
    {
        _selectedLevel = levelData;
        SceneManager.LoadScene("GameScene");
    }

    public static LevelData GetSelectedLevel()
    {
        return _selectedLevel;
    }
}