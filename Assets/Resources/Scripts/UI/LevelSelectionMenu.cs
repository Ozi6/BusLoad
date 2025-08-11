using UnityEngine;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] private LevelData[] availableLevels;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonsParent;

    private void Start()
    {
        foreach (var level in availableLevels)
        {
            var buttonObj = Instantiate(levelButtonPrefab, buttonsParent);
            var levelButton = buttonObj.GetComponent<LevelButton>();
            levelButton.Setup(level);
        }
    }
}