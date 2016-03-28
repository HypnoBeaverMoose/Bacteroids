using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour 
{

    public delegate void PlayerKilled();
    public event PlayerKilled OnPlayerKilled;
    public delegate void ColorChanged(Color newColor);
    public event ColorChanged OnColorChanged;

    public float EnergyThreshold = 30;
    public bool NoDNA { get; private set; }
    public float Force { get; private set; }
    public float Angle { get; private set; }
    public float Energy { get { return _energy; } set { _energy = Mathf.Clamp(value, 0, _startingEnergy); } }
    public float MaxEnergy { get { return _startingEnergy; } }

    public Color Color
    {
        get { return _color; }
        set 
        {
            _color = value;
            _playerSprite.color = _color;
            _engineParticles.startColor = _color;
            if (OnColorChanged != null)
            {
                OnColorChanged(_color);
            }
        }
    }

    [SerializeField]
    private float _boostTimeout;
    [SerializeField]
    private float _noEnergyTimePenalty;
    [SerializeField]
    private float _noEnergyBonus;
    [SerializeField]
    private float _startingEnergy;
    [Space(10)]
    [SerializeField]
    private float _idleEnergyBonus;
    [SerializeField]
    private float _shootEnergyCost;
    [SerializeField]
    private float _moveEnergyCost;
    [SerializeField]
    private float _rotateEnergyCost;
    [Space(10)]
    [SerializeField]
    private float _forceMultiplier;
    [SerializeField]
    private float _torqueMultiplier;
    [SerializeField]
    private ParticleSystem _engineParticles;
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    private SpriteRenderer _playerSprite;


    private Rigidbody2D _rigidbody;
    private float _energy;
    private float _lastBoost = 0;
    private Color _color = Color.white;
	void Start () 
    {
        NoDNA = false;
        _lastBoost = Time.time;
        Force = 0;
        Angle = 0;
        Energy = _startingEnergy;
        _rigidbody = GetComponent<Rigidbody2D>();
	}
    private IEnumerator NoDNARoutine()
    {
        if (Time.time - _lastBoost > _boostTimeout)
        {
            Energy += _noEnergyBonus;
            _lastBoost = Time.time;
        }
        yield return new WaitForSeconds(_noEnergyTimePenalty);
        NoDNA = false;
    }
	
	void Update () 
    {
        if (Energy <= 0)
        {
            if (OnPlayerKilled != null)
            {
                OnPlayerKilled();
            }
        }

        Energy += _idleEnergyBonus * Time.deltaTime;
        Force = Mathf.Max(0, Input.GetAxis("Vertical"));
        Angle = Input.GetAxis("Horizontal");

        if (Energy < EnergyThreshold && !NoDNA)
        {
            NoDNA = true;
            StartCoroutine(NoDNARoutine());            
        }

        if (!NoDNA)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var go = Instantiate(_projectilePrefab, transform.position + transform.up * 0.5f, transform.localRotation) as GameObject;
                go.GetComponent<Projectile>().Color = Color;
                Energy -= _shootEnergyCost;
            }

            if (Force > 0)
            {
                _engineParticles.Emit(1);
            }
        }
	}

    void FixedUpdate()
    {
        if (!NoDNA)
        {
            _rigidbody.AddForce(transform.up * Force * _forceMultiplier);
            _rigidbody.AddTorque(-Angle * _torqueMultiplier);

            Energy -= Time.fixedDeltaTime * (Mathf.Abs(Force * _moveEnergyCost) + Mathf.Abs(Angle * _rotateEnergyCost));
        }
    }

    
}
