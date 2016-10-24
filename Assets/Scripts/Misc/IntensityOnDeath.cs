using UnityEngine;
using System.Collections;

public class IntensityOnDeath : MonoBehaviour {

    [SerializeField]
    private float _colorDutation;
    [SerializeField]
    private float _colorIntentisy;
    [SerializeField]
    private float _deathDuration;
    [SerializeField]
    private float _deathIntentisy;
    [SerializeField]
    private UnityStandardAssets.ImageEffects.NoiseAndGrain _grain;
    // Use this for initialization
    private bool running = false;
    void Start ()
    {
        Player.PlayerSpawned += OnPlayerSpawned;
	}

    private void OnPlayerSpawned(Player obj)
    {
        obj.PlayerKilled += OnPlayerKilled;
        obj.ColorChanged += OnColorChanged;
        if (!running)
        {
            StartCoroutine(SpikeIntensity(Color.white, _deathDuration, _deathIntentisy));
        }
    }

    private void OnColorChanged(Color obj)
    {
        if (!running)
        {
            StartCoroutine(SpikeIntensity(obj, _colorDutation, _colorIntentisy));
        }
    }

    private void OnPlayerKilled()
    {
        if (!running)
        {
            StartCoroutine(SpikeIntensity(Color.white, _deathDuration, _deathIntentisy));
        }
    }

    private IEnumerator SpikeIntensity(Color color, float duration, float intensity)
    {
        running = true;
        _grain.intensities = new Vector3(color.r, color.g, color.b);
        float velocity = 0;
        float originalValue = _grain.intensityMultiplier;
        _grain.intensityMultiplier = intensity;
        while (Mathf.Abs(_grain.intensityMultiplier - originalValue) > 0.1f)
        {
            _grain.intensityMultiplier = Mathf.SmoothDamp(_grain.intensityMultiplier, originalValue, ref velocity, duration);
            yield return null;
        }
        _grain.intensityMultiplier = originalValue;
        _grain.intensities = Vector3.one;
        running = false;
    }  
}
