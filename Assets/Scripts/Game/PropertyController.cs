using UnityEngine;
using System.Collections;

public class PropertyController : MonoBehaviour
{

    [SerializeField]
    private AnimationCurve WaveSize;
    [SerializeField]
    private AnimationCurve GrowthRate;
    [SerializeField]
    private AnimationCurve Speed;
    [SerializeField]
    private AnimationCurve MutationProbability;
    [SerializeField]
    private AnimationCurve StartSize;
    [SerializeField]
    private AnimationCurve MutateTime;
    [SerializeField]
    private AnimationCurve ColorCount;

    public float SampleGrowthRate(int wave)
    {
        return GrowthRate.Evaluate(wave - 1);
    }

    public float SampleSpeed(int wave)
    {
        return Speed.Evaluate(wave - 1);
    }

    public float SampleMutation(int wave)
    {
        return MutationProbability.Evaluate(wave);
    }

    public int SampleWaveSize(int wave)
    {
        return (int)WaveSize.Evaluate(wave - 1);
    }

    public float SampleStartRadius(int wave)
    {
        return StartSize.Evaluate(wave - 1);
    }

    public float SampleMutateTime(int wave)
    {
        return MutateTime.Evaluate(wave - 1);
    }

    public int SampleColorCount(int wave)
    {
        return (int)ColorCount.Evaluate(wave - 1);
    }
    public bool CanMutate(int wave)
    {
        return SampleMutation(wave - 1) > 0;
    }


}