using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;
public enum SoundType
{
    PlayerConsume,
    PlayerChangeColor,
    PlayerKilled,
    PlayerTurboOn,
    BacteriaHitMutated,
    BacteriaHit,
    BacteriaMutate,
    BacteriaSplit,
    BacteriaDie,
    PlayerSpawn,
    PlayerShoot
}

public class AudioController : MonoBehaviour
{
    private static AudioController _intance = null;
    public static AudioController Instance
    {
        get
        {
            if (_intance == null)
            {
                _intance = FindObjectOfType<AudioController>();
                if (_intance == null)
                {
                    var go = new GameObject("Audio");
                    _intance = go.AddComponent<AudioController>();
                }
            }
            return _intance;
        }
    }

    [Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip[] Clips;
        public float Volume;
        public float Pitch;
        public float PitchDelta;
    }
    [SerializeField]
    private Sound[] _sounds;
    [SerializeField]
    private GameObject _sourcePrefab;

    private int _currentPriotiy = 0;
    
    private List<AudioSource> _idleSources = new List<AudioSource>();
    private List<AudioSource> _playingSources = new List<AudioSource>();

    void Start()
    {        
    }

    public void PlaySound(SoundType type)
    {
        PlaySound(type, Vector3.zero);
    }

    public void PlaySound(SoundType type, Vector3 position)
    {
        var sound = _sounds.First(s => s.Type == type);
        if (sound != null)
        {        
            AudioSource source = GetSource();
            source.transform.position = position;
            source.volume = sound.Volume;
            source.pitch = sound.Pitch + Random.Range(-sound.PitchDelta, sound.PitchDelta);
            source.PlayOneShot(sound.Clips[Random.Range(0, sound.Clips.Length)]);
        }
    }

    void Update()
    {
        for(int i = 0; i < _playingSources.Count; i++)
        {
            if (!_playingSources[i].isPlaying)
            {
                _idleSources.Add(_playingSources[i]);
                _playingSources.RemoveAt(i--);
            }
        }
    }

    private AudioSource GetSource()
    {
        AudioSource source;
        if (_idleSources.Count > 0)
        {
            source = _idleSources.Last();
            _idleSources.RemoveAt(_idleSources.Count - 1);
        }
        else
        {
            source = GameObject.Instantiate<GameObject>(_sourcePrefab).GetComponent<AudioSource>();
            source.transform.SetParent(transform);
        }

        _playingSources.Add(source);
        return source;
    }
    
}
