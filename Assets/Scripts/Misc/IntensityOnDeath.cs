using UnityEngine;
using System.Collections;

public class IntensityOnDeath : MonoBehaviour {

    [SerializeField]
    private float _duration;
    [SerializeField]
    private float _amount;
    [SerializeField]
    private UnityStandardAssets.ImageEffects.NoiseAndGrain _grain;
    // Use this for initialization
    void Start ()
    {
        Player.OnPlayerSpawned += OnPlayerSpawned;
	}

    private void OnPlayerSpawned(Player obj)
    {
        obj.OnPlayerKilled += OnPlayerKilled;
    }

    private void OnPlayerKilled()
    {
        StartCoroutine(SpikeIntensity());
    }

    private IEnumerator SpikeIntensity()
    {
        float velocity = 0;
        float originalValue = _grain.intensityMultiplier;
        _grain.intensityMultiplier = _amount;
        while (Mathf.Abs(_grain.intensityMultiplier - originalValue) > 0.1f)
        {
            _grain.intensityMultiplier = Mathf.SmoothDamp(_grain.intensityMultiplier, originalValue, ref velocity, _duration);
            yield return null;
        }
        _grain.intensityMultiplier = originalValue;
    }  
}
