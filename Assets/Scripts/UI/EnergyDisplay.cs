using UnityEngine;
using System.Collections;

public class EnergyDisplay : MonoBehaviour 
{

    [SerializeField]
    private Transform _energyBar;
    [SerializeField]
    private float _smoothTime;

    private Player _player;
    private Vector3 _target;
    private Vector3 _velocity;

	void Start () 
    {
        _player = FindObjectOfType<Player>();
        _target = Vector3.one;
	}
	
	void Update () 
    {
        if (_player != null)
        {
            _target.x = _player.Energy / _player.MaxEnergy;
            _energyBar.localScale = Vector3.SmoothDamp(_energyBar.localScale, _target, ref _velocity, _smoothTime);
        }
        else
        {
            _player = FindObjectOfType<Player>();
        }
	}
}
