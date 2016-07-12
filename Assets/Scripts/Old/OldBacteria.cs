using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CompoundSoftBody))]
public class OldBacteria : MonoBehaviour 
{
    public static int EnergyMultiplier = 200;
    public static int DamageMultiplier = 20;
    
    [SerializeField]
    private float _moveForce;
    [SerializeField]
    private float _addThreshold = 0.6f;
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
    public Color Color { get { return _color; } set { _color = value; if (_material != null) _material.color = _color; } }
    public bool isEnergy { get  { return gameObject.layer == LayerMask.NameToLayer("Energy"); } }
    public bool isBacteria { get { return gameObject.layer == LayerMask.NameToLayer("Bacteria"); } }

    private CompoundSoftBody _softbody;
    private Material _material;
    private float _growTimer;
    private float _moveTimer;
    private Color _color;
    private List<Rigidbody2D> _childrenNearPlayer = new List<Rigidbody2D>();
    private bool _nearPlayer { get { return _childrenNearPlayer.Count > 0; } }

    private void Awake()
    {
        _color = Color.black;
        _softbody = gameObject.GetComponent<CompoundSoftBody>();
        _material = gameObject.GetComponent<Renderer>().material;        
        _growTimer = Random.Range(_growIntervalMin, _growIntervalMax);
        //_moveTimer = Random.Range(_growIntervalMin, _growIntervalMax) / 2;
    }

    private void Start()
    {
        _softbody.Init();
        //SetLayer(_thickness.Evaluate(_softbody.Size) > 0 ? LayerMask.NameToLayer("Bacteria") : LayerMask.NameToLayer("Energy"));
        _material.color = _color;
    }    

    public void OnCollisionEnterChild(Rigidbody2D child, Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            child.AddForceAtPosition(collision.relativeVelocity.magnitude *
                collision.contacts[0].normal / 2, collision.contacts[0].point, ForceMode2D.Impulse);

            //var color = collision.gameObject.GetComponent<Projectile>().Color;
            //if (Color == Color.white || color == Color || color == Color.white)
            {
              //  StartCoroutine(Split(child, collision));
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (isEnergy)
            {
                collision.gameObject.GetComponent<Player>().Energy += Energy;
                collision.gameObject.GetComponent<Player>().Color = Color;
                //Physics2D.IgnoreCollision(child.GetComponent<CircleCollider2D>(), collision.collider);
                Destroy(gameObject);                              
            }
            else
            {
                var dir = Random.insideUnitCircle.normalized;
                if(Vector3.Dot(dir, -collision.contacts[0].normal) < 0)
                {
                    dir *=-1;
                }
                collision.gameObject.GetComponent<Player>().Color = Color;
                collision.rigidbody.AddForceAtPosition(_hitbackForce * _softbody.Size * dir, collision.contacts[0].point, ForceMode2D.Impulse);
                child.AddForceAtPosition(-_hitbackForce * _softbody.Size * dir, collision.contacts[0].point, ForceMode2D.Impulse);

                var explosion = Instantiate(Resources.Load<ParticleSystem>("explosion"), collision.contacts[0].point, Quaternion.identity) as ParticleSystem;
                explosion.startColor = Color;
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
        if (index == -1)
        {
            yield break;
        }
        float chunkSize = Mathf.Lerp(0.1f, 0.2f, Vector3.Dot(collision.relativeVelocity.normalized, -collision.contacts[0].normal));
        if (_softbody.Vertices > 4)
        {
            _softbody.RemoveNode(child, chunkSize);
        }
        else
        {
            _softbody.Grow(-chunkSize);
        }

        _softbody.ChildAtIndex(index).AddForceAtPosition(collision.relativeVelocity.magnitude * 
            collision.contacts[0].normal / 2, collision.contacts[0].point, ForceMode2D.Impulse);
        //yield return new WaitForSeconds(0.1f);
        
        var bacteria = FindObjectOfType<GameController>().SpawnBacteria(collision.contacts[0].point, chunkSize, 4, Color);
        var dir = Random.insideUnitCircle.normalized;
        yield return null;
        if (bacteria != null)
        {
            bacteria.GetComponent<Rigidbody2D>().AddForce(dir * 10, ForceMode2D.Impulse);
        }

    }

    private void Update()
    {
        //float thickness = _thickness.Evaluate(_softbody.Size);
        //_material.SetFloat("_Thickness", thickness);
        //if (!_nearPlayer && isEnergy && thickness > 0)
        //{
        //    SetLayer(LayerMask.NameToLayer("Bacteria"));
        //}
        //else if (isBacteria && thickness == 0)
        //{
        //    SetLayer(LayerMask.NameToLayer("Energy"));
        //}
        //_moveTimer -= isEnergy ? 0 : Time.deltaTime;
        //_growTimer -= _softbody.Size < _maxSize ? Time.deltaTime : 0;

        //if (_moveTimer < 0)
        //{
        //    Vector3 direction = Random.insideUnitCircle.normalized;
        //    float mult = 1;
        //    if (_nearPlayer && FindObjectOfType<Player>() != null)
        //    {
        //        mult = 2;
        //        direction = (FindObjectOfType<Player>().transform.position - transform.position).normalized;
        //    }
        //    var hit = Physics2D.Raycast(transform.position + direction * _softbody.Size, -direction, 100, LayerMask.GetMask("Bacteria"));
        //    hit.rigidbody.AddForce(direction * _moveForce * mult, ForceMode2D.Impulse);
        //    _moveTimer = Random.Range(_growIntervalMin, _growIntervalMax) / 2;
        //}

        if (_growTimer < 0.0f && _softbody.Vertices < _maxVertices)
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

    public void OnTriggerEnterChild(Rigidbody2D rigidbody2D, Collider2D other)
    {
        if (other.CompareTag("Player") && !_childrenNearPlayer.Contains(rigidbody2D))
        {
            _childrenNearPlayer.Add(rigidbody2D);
        }
    }

    public void OnTriggerExitChild(Rigidbody2D rigidbody2D, Collider2D other)
    {
        if (other.CompareTag("Player") && _childrenNearPlayer.Contains(rigidbody2D))
        {
            _childrenNearPlayer.Remove(rigidbody2D);
        }
    }
}