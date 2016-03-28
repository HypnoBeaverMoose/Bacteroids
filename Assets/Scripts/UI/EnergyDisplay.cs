using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyDisplay : MonoBehaviour 
{

    [SerializeField]
    private GameObject _noEnergy;
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

    private void OnColorChanged(Color newColor)
    {
        foreach (var item in GetComponentsInChildren<Graphic>())
        {           
            item.color = new Color(newColor.r, newColor.g, newColor.b, item.color.a);
        }
    }
	
	void Update () 
    {
        if (_player != null)
        {
            _target.x = _player.Energy / _player.MaxEnergy;
            _energyBar.localScale = Vector3.SmoothDamp(_energyBar.localScale, _target, ref _velocity, _smoothTime);
            _noEnergy.transform.position = _player.transform.position - Vector3.up * 0.5f;
            if (_player.NoDNA && !_noEnergy.activeSelf)
            {
                _noEnergy.SetActive(true);
            }
            else if ((!_player.NoDNA || _player.Energy <= 0) && _noEnergy.activeSelf)
            {
                _noEnergy.SetActive(false);
            }
           
        }
        else
        {
            _target.x = 0;
            _energyBar.localScale = Vector3.SmoothDamp(_energyBar.localScale, _target, ref _velocity, _smoothTime);
            _player = FindObjectOfType<Player>();
            if (_player != null)
            {
              //  _player.OnColorChanged += OnColorChanged;
            }
        }
	}
}
