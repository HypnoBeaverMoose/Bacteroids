using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static event Action<Player> PlayerSpawned;
    public event Action PlayerKilled;
    public event Action<Color> ColorChanged;

    public bool UseEnergy {  get { return _useEnergy; } }
    public bool HasEnergy { get; private set; }
    public float Force { get; private set; }
    public float Angle { get; private set; }
    public float Energy { get { return _energy; } set { _energy = Mathf.Clamp(value, 0, _startingEnergy); } }
    public float MaxEnergy { get { return _startingEnergy; }  }

    public Color Color
    {
        get { return _color; }
        set 
        {
            if (_color != value)
            {
                _color = value;
                _playerSprite.color = _color;
                _engineParticles.startColor = _color;
                _bubbleParticles.startColor = _color;
                AudioController.Instance.PlaySound(SoundType.PlayerChangeColor, transform.position);
                if (ColorChanged != null)
                {
                    ColorChanged(_color);
                }
            }
        }
    }
    [SerializeField]
    private AudioSource _engineAudio;
    [SerializeField]
    private bool _useEnergy;
    [SerializeField]
    private float _invincibleTimeout;
    [SerializeField]
    private float _boostTimeout;
    [SerializeField]
    private float _noEnergyTimePenalty;
    [SerializeField]
    private float _noEnergyBonus;
    [SerializeField]
    private float _startingEnergy;
    [SerializeField]
    private float _energyThreshold;

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
    private ParticleSystem _explosion;
    [SerializeField]
    private ParticleSystem _engineParticles;
    [SerializeField]
    private ParticleSystem _bubbleParticles;
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    private SpriteRenderer _playerSprite;
    [SerializeField]
    private float _rotationTurbo;
    [SerializeField]
    private float _movementTurbo;
    [SerializeField]
    private bool _mouseRotaion;
    [SerializeField]
    private float _rotationSpeed;

    [SerializeField]
    private float _maxDrag;
    [SerializeField]
    private float _minDrag;
    [SerializeField]
    private float _dragThreshold;


    private float turbo { get { return _turbo ? _movementTurbo : 1; } }

    private Rigidbody2D _rigidbody;
    private float _energy;
    private float _lastBoost = 0;
    private Color _color = Color.white;
    private bool _invincible = false;
    private bool _turbo = false;
    private float _realMaxVelocity;
	void Start () 
    {
        if (GetComponent<Wrappable>() != null)
        {
            GetComponent<Wrappable>().Size = 0.3f;
        }
        HasEnergy = true;
        _lastBoost = Time.time;
        Force = 0;
        Angle = 0;
        Energy = _startingEnergy;
        _rigidbody = GetComponent<Rigidbody2D>();
        StartCoroutine(Invincibility());
        if (PlayerSpawned != null)
        {
            PlayerSpawned(this);
        }
        if (ColorChanged != null)
        {
            ColorChanged(Color);
        }
        AudioController.Instance.PlaySound(SoundType.PlayerSpawn);
	}

    private IEnumerator NoEnergyRoutine()
    {
        if (Time.time - _lastBoost > _boostTimeout)
        {
            Energy += _noEnergyBonus;
            _lastBoost = Time.time;
        }
        yield return new WaitForSeconds(_noEnergyTimePenalty);
        HasEnergy = true;
    }
	
	void Update () 
    {
        Energy += _idleEnergyBonus * Time.deltaTime;
        Force = Input.GetAxis("Vertical");
        Angle = Input.GetAxis("Horizontal") * Time.fixedDeltaTime; ;

        if (_useEnergy && (Energy < _energyThreshold && HasEnergy))
        {
            HasEnergy = false;
            StartCoroutine(NoEnergyRoutine());
        }

        if (_useEnergy && (Energy > _energyThreshold && !HasEnergy))
        {
            HasEnergy = true;
        }
            
        if (!_useEnergy || HasEnergy)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                var go = Instantiate(_projectilePrefab, transform.position + transform.up * 0.5f, transform.localRotation) as GameObject;
                go.GetComponent<Projectile>().Color = Color;
                AudioController.Instance.PlaySound(SoundType.PlayerShoot, transform.position);
                if (!_invincible)
                {
                    Energy -= _shootEnergyCost;
                }
            }
        }
        _turbo = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.LeftShift) && Force > 0)
        {
            ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Random, _rigidbody.position, Color);
            AudioController.Instance.PlaySound(SoundType.PlayerTurboOn, transform.position);
            Force *= 1.5f;
        }
        else
        {
            _engineParticles.Emit((int)(Force * 10 * (_turbo ? 2 : 0.8f)));
        }
        _bubbleParticles.Emit((int)(_rigidbody.velocity.magnitude * (_turbo ? 2 : 0.8f)));
        GetComponent<EngineSound>().speed = Force * 10 * turbo;
    }

    void FixedUpdate()
    {

       
        _rigidbody.drag = Mathf.Abs(Force) > _dragThreshold ? _maxDrag : _minDrag;
        if (Force < 0)
        {
            Force *= 0.7f;
        }
        _rigidbody.AddForce(transform.up * Force * _forceMultiplier * turbo);

        if (_mouseRotaion)
        {
            var dir = (new Vector3(CursorControl.Position.x, CursorControl.Position.y, transform.position.z) - transform.position).normalized;
            float angle = Vector2.Angle(transform.up, dir);
            var cross = Vector3.Cross(transform.up.normalized, dir.normalized);
            _rigidbody.rotation += Mathf.Sign(cross.z) * Vector2.Angle(transform.up, dir) * _rotationSpeed * turbo;
        }
        else
        {
            _rigidbody.rotation += Angle * _torqueMultiplier * (_turbo ? _rotationTurbo : 1);
        }

        if (HasEnergy || !_useEnergy)
        {
            Energy -= Time.fixedDeltaTime * (Mathf.Abs(Force * _moveEnergyCost) + Mathf.Abs(Angle * _rotateEnergyCost));
        }
        
    }

    public void Damage(Bacteria bacteria)
    {
        if (!_invincible && PlayerKilled != null)
        {
            PlayerKilled();
        }
    }

    public void Consume(Energy energy)
    {
        if (GameController.Instance.Spawn.CurrentWave > 1)
        {
            Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.EnergyConsumedByPlayer);
        }
        GameController.Instance.Score += energy.Score;
        Energy += energy.Score;
        AudioController.Instance.PlaySound(SoundType.PlayerConsume, transform.position);
        if (Color != energy.Color)
        {
            Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.PlayerChangesColor);
        }
        Color = energy.Color;
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

    public void Kill()
    {
        ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Huge,transform.position, Color);
        Destroy(gameObject);
        AudioController.Instance.PlaySound(SoundType.PlayerKilled, transform.position);

    }

    private void OnDestroy()
    {
        PlayerKilled = null;
        ColorChanged = null;
    }
}
