using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public float Force { get; private set; }
    public float Angle { get; private set; }
    public float Energy { get;  set; }
    public float MaxEnergy { get { return _startingEnergy; } }

    [SerializeField]
    private float _startingEnergy;
    [Space(10)]
    [SerializeField]
    private float _idleEnergyCost;
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

    private Rigidbody2D _rigidbody;


	void Start () 
    {
        Force = 0;
        Angle = 0;
        Energy = _startingEnergy;
        _rigidbody = GetComponent<Rigidbody2D>();
	}
	
	
	void Update () 
    {
        Force = Mathf.Max(0, Input.GetAxis("Vertical"));
        Angle = Input.GetAxis("Horizontal");
        Energy -= _idleEnergyCost * Time.deltaTime;
        if (Force > 0)
        {
            _engineParticles.Emit(1);            
        }

        if(Input.GetKeyDown( KeyCode.Space))    
        {
            Instantiate(_projectilePrefab, transform.position + transform.up* 0.5f, transform.localRotation);
            Energy -= _shootEnergyCost;
        }

        if (Energy <= 0)
        {
            Destroy(gameObject);
        }

	}

    void FixedUpdate()
    {
        _rigidbody.AddForce(transform.up * Force * _forceMultiplier);
        _rigidbody.AddTorque(-Angle * _torqueMultiplier);

        Energy -= Time.fixedDeltaTime * (Mathf.Abs(Force * _moveEnergyCost) + Mathf.Abs(Angle * _rotateEnergyCost));
    }

    
}
