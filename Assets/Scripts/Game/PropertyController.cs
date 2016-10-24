using UnityEngine;
using System.Collections;

public class PropertyController : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve GrowthRate;
    [SerializeField]
    private AnimationCurve Speed;
    [SerializeField]
    private AnimationCurve MutationProbability;

    public float SampleGrowthRate(int kills)
    {
        return GrowthRate.Evaluate(kills);
    }

    public float SampleSpeed(int kills)
    {
        return Speed.Evaluate(kills);
    }

    public float SampleMutation(int kills)
    {
        return MutationProbability.Evaluate(kills);
    }

    public bool CanMutate(int kills)
    {
        return SampleMutation(kills) > 0;
    }
}