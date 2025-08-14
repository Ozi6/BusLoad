using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    public GameObject gameOverUI;
    public Button retryButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLevel);
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void CheckGameOverConditions()
    {
        if (QueueManager.Instance.IsQueueFull())
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    private void RetryLevel()
    {
        Time.timeScale = 1f;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        List<GameObject> movingObjects = MovementManager.Instance.GetAllMovingObjects();
        foreach (GameObject obj in movingObjects)
        {
            if (obj != null)
            {
                MovementManager.Instance.CancelMovement(obj);
                Destroy(obj);
            }
        }
        QueueManager.Instance.EmptyQueue();
        BusController.Instance.ResetBusSystem();
        GameManager.Instance.RespawnPassengers();
    }
}