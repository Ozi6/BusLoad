using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private LevelData levelData;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        LevelManager.SelectLevel(levelData);
    }

    public void Setup(LevelData data)
    {
        levelData = data;
        //GetComponentInChildren<TextMeshPro>().text = data.levelName;
    }
}