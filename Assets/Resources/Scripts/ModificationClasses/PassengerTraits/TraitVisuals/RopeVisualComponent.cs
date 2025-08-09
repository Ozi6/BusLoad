using UnityEngine;

public class RopeVisualComponent : TraitVisualComponent
{
    [SerializeField] private GameObject[] ropeSegments;
    private int currentRopeCount;

    public override void UpdateVisual(PassengerTrait trait)
    {
        RopedTrait ropeTrait = (RopedTrait)trait;
        int ropeCount = ropeTrait.GetRopeCount();

        for (int i = 0; i < ropeSegments.Length; i++)
            ropeSegments[i].SetActive(i < ropeCount);
        currentRopeCount = ropeCount;
    }

    public override void ResetVisual()
    {
        foreach (var segment in ropeSegments)
            segment.SetActive(false);
        currentRopeCount = 0;
    }
}