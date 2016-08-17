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
    public float ExplodeStatus { get { return 1.0f - _explodeTimer / _explodeCooldown; } }
    public bool NoDNA { get; private set; }
    public float Force { get; private set; }
    public float Angle { get; private set; }
    public float Energy { get { return _energy; } set { _energy = Mathf.Clamp(value, 0, _startingEnergy); } }
    public float MaxEnergy { get { return _startingEnergy; }  }

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
    private float _invincibleTimeout;
    [SerializeField]
    private float _explodeCooldown;
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
    private float _explodeTimer = 0;
    private bool _invincible = false;
	void Start () 
    {
        if (GetComponent<Wrappable>() != null)
        {
            GetComponent<Wrappable>().Size = 0.3f;
        }

        _explodeTimer = 0.25f * _explodeCooldown;
        NoDNA = false;
        _lastBoost = Time.time;
        Force = 0;
        Angle = 0;
        Energy = _startingEnergy;
        _rigidbody = GetComponent<Rigidbody2D>();
        StartCoroutine(Invincibility());
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
        Energy += _idleEnergyBonus * Time.deltaTime;
        Force = Mathf.Max(0, Input.GetAxis("Vertical"));
        Angle = Input.GetAxis("Horizontal") * Time.fixedDeltaTime; ;

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
                if (!_invincible)
                {
                    Energy -= _shootEnergyCost;
                }
            }
        }

        if (Force > 0)
        {
            _engineParticles.Emit(1);
        }

        _explodeTimer = Mathf.Max(_explodeTimer - Time.deltaTime, 0);
	}

    void FixedUpdate()
    {
        _rigidbody.AddForce(transform.up * Force * _forceMultiplier);
        _rigidbody.rotation += Angle * _torqueMultiplier;

        if (!NoDNA)
        {
            Energy -= Time.fixedDeltaTime * (Mathf.Abs(Force * _moveEnergyCost) + Mathf.Abs(Angle * _rotateEnergyCost));
        }
    }

    public void Damage(Bacteria bacteria)
    {
        if (!_invincible)
        {
            if (OnPlayerKilled != null)
            {
                OnPlayerKilled();
            }
        }
    }

    private IEnumerator Invincibility()
    {
        _invincible = true;
        float timer = _invincibleTimeout;
        while (timer > 0)
        {
            var color = _playerSprite.color;
            color.a = 0.5f + 0.5f * Mathf.Sin(Time.time * 10);
            _playerSprite.color = color;
            timer -= Time.deltaTime;
            yield return null;
        }
        _playerSprite.color = new Color(Color.r, Color.g, Color.b, 1.0f);
        _invincible = false;
    }
}
