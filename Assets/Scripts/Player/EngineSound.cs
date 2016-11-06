using UnityEngine;
using System.Collections;

public class EngineSound : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;
    [SerializeField]
    private float _maxSpeed;
    [SerializeField]
    private float _maxPitch;
    [SerializeField]
    private float _maxVolume;

    private float _speed = 0;
    private float _basePitch;

    public float speed { get { return _speed; } set { _speed = value; } }

    // Use this for initialization
    void Start ()
    {
	}
    
    void Update()
    {
        Debug.Log(_speed);
        float val = _speed / _maxSpeed;
        _source.pitch = Mathf.Lerp(_source.pitch, Mathf.Lerp(_basePitch, _maxPitch, val), 0.8f);
        _source.volume = Mathf.Lerp(_source.volume, Mathf.Lerp(0.0f, _maxVolume, val), 0.8f);
    }
}
