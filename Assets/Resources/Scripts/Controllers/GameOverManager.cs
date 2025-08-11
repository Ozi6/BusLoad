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
        if (!QueueManager.Instance.IsQueueFull())
            return;

        if (HasValidPassengerForAnyBus())
            return;

        TriggerGameOver();
    }

    private bool HasValidPassengerForAnyBus()
    {
        List<Passenger> interactablePassengers = GetAllInteractablePassengers();

        if (interactablePassengers.Count == 0)
            return false;

        foreach (Passenger passenger in interactablePassengers)
            if (passenger.CanBoardBus(BusController.Instance.CurrentBus))
                return true;

        return false;
    }

    private List<Passenger> GetAllInteractablePassengers()
    {
        List<Passenger> interactablePassengers = new List<Passenger>();

        foreach (var kvp in GameManager.Instance.gridObjects)
            if (kvp.Value is Passenger passenger)
                if (IsPassengerInteractable(passenger))
                    interactablePassengers.Add(passenger);

        return interactablePassengers;
    }

    private bool IsPassengerInteractable(Passenger passenger)
    {
        if (!passenger.IsInteractable)
            return false;

        foreach (PassengerTrait trait in passenger.traits)
            if (trait is RopedTrait ropedTrait)
                if (ropedTrait.GetRopeCount() > 0)
                    return false;

        return true;
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

        QueueManager.Instance.EmptyQueue();
        GameManager.Instance.RespawnPassengers();
    }
}