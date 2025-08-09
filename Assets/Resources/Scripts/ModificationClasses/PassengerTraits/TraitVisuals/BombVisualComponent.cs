using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BombVisualComponent : TraitVisualComponent
{
    [Header("Bomb Visual Elements")]
    [SerializeField] private GameObject bombInactiveVisual;
    [SerializeField] private GameObject bombActiveVisual;
    [SerializeField] private TextMeshPro countdownText;

    [Header("Visual Effects")]
    [SerializeField] private Color normalCountdownColor = Color.white;
    [SerializeField] private Color urgentCountdownColor = Color.red;
    [SerializeField] private int urgentThreshold = 3;
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;

    private BombedTrait bombTrait;
    private bool isPulsing = false;

    public override void UpdateVisual(PassengerTrait trait)
    {
        bombTrait = (BombedTrait)trait;

        bool isActivated = bombTrait.IsActivated();
        bool isExploded = bombTrait.IsExploded();
        int currentCountdown = bombTrait.GetCurrentCountdown();

        if (bombInactiveVisual != null) bombInactiveVisual.SetActive(!isActivated && !isExploded);
        if (bombActiveVisual != null) bombActiveVisual.SetActive(isActivated && !isExploded);
        //if (bombExplodedVisual != null) bombExplodedVisual.SetActive(isExploded);

        if (countdownText != null)
        {
            if (isActivated && !isExploded)
            {
                countdownText.text = currentCountdown.ToString();
                countdownText.gameObject.SetActive(true);

                Color textColor = currentCountdown <= urgentThreshold ? urgentCountdownColor : normalCountdownColor;
                countdownText.color = textColor;

                if (currentCountdown <= urgentThreshold && enablePulseEffect && !isPulsing)
                {
                    isPulsing = true;
                    StartCoroutine(PulseEffect());
                }
            }
            else
            {
                countdownText.gameObject.SetActive(false);
                isPulsing = false;
            }
        }
    }

    public override void ResetVisual()
    {
        if (bombInactiveVisual != null) bombInactiveVisual.SetActive(true);
        if (bombActiveVisual != null) bombActiveVisual.SetActive(false);
        //if (bombExplodedVisual != null) bombExplodedVisual.SetActive(false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.color = normalCountdownColor;
        }

        isPulsing = false;
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator PulseEffect()
    {
        while (isPulsing && bombTrait != null && bombTrait.IsActivated() && !bombTrait.IsExploded())
        {
            if (countdownText != null)
            {
                float pulseValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                float scale = Mathf.Lerp(1f, 1.3f, pulseValue);
                countdownText.transform.localScale = Vector3.one * scale;
                Color color = countdownText.color;
                color.a = Mathf.Lerp(0.7f, 1f, pulseValue);
                countdownText.color = color;
            }

            yield return null;
        }
        if (countdownText != null)
        {
            countdownText.transform.localScale = Vector3.one;
            Color color = countdownText.color;
            color.a = 1f;
            countdownText.color = color;
        }
    }
}