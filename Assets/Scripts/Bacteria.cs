using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Bacteria : MonoBehaviour 
{
    
    public float GrowSpeed = 1.0f;
    public int MaxVertices = 8;
    public float Energy { get; set; }
    
    [SerializeField]
    private GameObject _energy;
    [SerializeField]
    private float _startEnergy;
   
    private Rigidbody2D _rigidbody = null;
    private ISoftBody _softBody;

    private void Awake()
    {
        _softBody = gameObject.AddComponent<SimpleSoftBody>();
        _rigidbody = GetComponent<Rigidbody2D>();
        Energy = _startEnergy;

    }

    private void Start()
    {
        _softBody.Init();
    }    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody == null)
        {
            return;
        }
        float mass = collision.rigidbody != null ? collision.rigidbody.mass : _rigidbody.mass;
        Vector2 force = (collision.relativeVelocity * mass) / Time.fixedDeltaTime;
        
        _softBody.AddDeformingForce(collision.contacts[0].point,  - force / 2);
        
        if (collision.gameObject.CompareTag("Projectile"))
        {            
            _rigidbody.AddForceAtPosition(-force / 2, collision.contacts[0].point);
            
            var go = Instantiate<GameObject>(_energy);
            float damage = Random.Range(5, 20);
            this.Energy -= damage;
            
            go.GetComponent<Energy>().Amount = damage;
            go.transform.position = collision.contacts[0].point;
            //go.GetComponent<Rigidbody2D>().AddForceAtPosition(-force.magnitude * collision.contacts[0].normal, collision.contacts[0].point);
        }
    }

    void FixedUpdate()
    {
        _softBody.UpdateDeformation();
        _softBody.UpdateBody();

        if (_softBody.AverageVelocity.magnitude < 1)
        {
            _softBody.AddDeformingForce((Vector2)transform.position + Random.insideUnitCircle, 1.5f * Random.insideUnitCircle.normalized);
        }
    }

    private void Update()
    {
        if (Energy <= 0)
        {
            Destroy(gameObject);
        }
    }

}