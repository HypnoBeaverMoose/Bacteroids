using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CompoundSoftBody))]
public class Bacteria : MonoBehaviour 
{
    public static int EnergyMultiplier = 100;
    public static int DamageMultiplier = 30; 

    [SerializeField]
    private AnimationCurve _thickness;
    [SerializeField]
    private float _addThreshold = 0.6f;
    [SerializeField]
    private float _maxSize = 1.3f;
    [SerializeField]
    private int _maxVertices = 8;
    [SerializeField]
    private float _growIntervalMin = 2.0f;
    [SerializeField]
    private float _growIntervalMax = 4.0f;
    [SerializeField]
    private float _growAmount = 0.1f;
    [SerializeField]
    private float _hitbackForce = 3;

    public float Energy { get { return _softbody.Size * EnergyMultiplier; } }
    
    public bool isEnergy { get  { return gameObject.layer == LayerMask.NameToLayer("Energy"); } }
    public bool isBacteria { get { return gameObject.layer == LayerMask.NameToLayer("Bacteria"); } }

    private Rigidbody2D _rigidbody = null;
    private CompoundSoftBody _softbody;
    private Material _material;
    private float _growTimer;
    private bool _consumed = false;
    private void Awake()
    {
        _softbody = gameObject.GetComponent<CompoundSoftBody>();
        _material = gameObject.GetComponent<Renderer>().material;        
        _growTimer = Random.Range(_growIntervalMin, _growIntervalMax);
    }

    private void Start()
    {
        _softbody.Init();
        _rigidbody = GetComponent<Rigidbody2D>();
        SetLayer(_thickness.Evaluate(_softbody.Size) > 0 ? LayerMask.NameToLayer("Bacteria") : LayerMask.NameToLayer("Energy"));
        _material.color = Random.ColorHSV(0, 1, 1, 1, 0.5f, 1.0f);
    }    

    public void OnCollisionEnterChild(Rigidbody2D child, Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {            
            //Vector3 offset = new Vector3(-collision.relativeVelocity.y, collision.relativeVelocity.x, 0).normalized;
            StartCoroutine(Split(child, collision));

        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (isEnergy)
            {
                collision.gameObject.GetComponent<Player>().Energy += Energy;
                Destroy(gameObject);

            }
            else
            {
                var dir = Random.insideUnitCircle.normalized;
                if(Vector3.Dot(dir, -collision.contacts[0].normal) < 0)
                {
                    dir *=-1;
                }

                collision.rigidbody.AddForceAtPosition(_hitbackForce * _softbody.Size * dir, collision.contacts[0].point, ForceMode2D.Impulse);
                child.AddForceAtPosition(-_hitbackForce * _softbody.Size * dir, collision.contacts[0].point, ForceMode2D.Impulse);

                var explosion = Instantiate(Resources.Load<ParticleSystem>("explosion"), collision.contacts[0].point, Quaternion.identity) as ParticleSystem;
                explosion.startColor = _material.color;
                explosion.Emit(30);
                Destroy(explosion, 10);
                collision.gameObject.GetComponent<Player>().Energy -= _softbody.Size * DamageMultiplier;
            }
        }
    }

    private IEnumerator Split(Rigidbody2D child, Collision2D collision)
    {
        yield return null;
        int index = _softbody.ChildIndex(child);
        if (_softbody.Vertices > 4)
        {
            _softbody.RemoveNode(child, _growAmount);
        }
        else
        {            
            _softbody.Grow(-_growAmount);
        }        
        _softbody.ChildAtIndex(index).AddForceAtPosition(collision.relativeVelocity.magnitude * 
            collision.contacts[0].normal / 2, collision.contacts[0].point, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.1f);
        
        var bacteria = FindObjectOfType<GameController>().SpawnBacteria(collision.contacts[0].point, 0.1f, 4);
        var dir = Random.insideUnitCircle;
        if (Vector3.Dot(dir, collision.relativeVelocity.normalized) < 0)
        {
            dir *= -1;
        }
        yield return null;
        bacteria.GetComponent<Rigidbody2D>().AddForce(dir * 10, ForceMode2D.Impulse);
        bacteria.GetComponent<Bacteria>()._material.color = _material.color;
    }

    private void Update()
    {
        float thickness = _thickness.Evaluate(_softbody.Size);
        _material.SetFloat("_Thickness", thickness);
        if (!_consumed && isEnergy && thickness > 0)
        {
            SetLayer(LayerMask.NameToLayer("Bacteria"));
        }
        else if (isBacteria && thickness == 0)
        {
            SetLayer(LayerMask.NameToLayer("Energy"));
        }

        _growTimer -= _softbody.Size < _maxSize ? Time.deltaTime : 0;
        if (_growTimer < 0.0f)
        {
            if (_softbody.Size < _addThreshold)
            {
                _softbody.Grow(_growAmount);
            }
            else
            {
                _softbody.AddNode(_growAmount);
            }
            _growTimer = Random.Range(_growIntervalMin, _growIntervalMax);
        }
    }

    private void SetLayer(int layer)
    {
        gameObject.layer = layer;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = layer;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _consumed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _consumed = false;
        }
    }

}