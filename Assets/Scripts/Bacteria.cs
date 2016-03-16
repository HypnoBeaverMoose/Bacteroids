using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CompoundSoftBody))]
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
    private CompoundSoftBody _softbody;

    private void Awake()
    {
        _softbody = gameObject.GetComponent<CompoundSoftBody>();
        _rigidbody = GetComponent<Rigidbody2D>();
        Energy = _startEnergy;

    }

    private void Start()
    {
        _softbody.Init();
    }    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody == null)
        {
            return;
        }
        float mass = collision.rigidbody != null ? collision.rigidbody.mass : _rigidbody.mass;
        Vector2 force = -(collision.relativeVelocity * mass) / Time.fixedDeltaTime;


        if (collision.gameObject.CompareTag("Projectile") && collision.rigidbody != null)
        {
            _rigidbody.AddForceAtPosition(force, collision.contacts[0].point);

            var go = Instantiate<GameObject>(_energy);
            float damage = Random.Range(5, 20);
            this.Energy -= damage;
            go.GetComponent<Energy>().Amount = damage;
            go.transform.position = collision.contacts[0].point;
            go.GetComponent<Rigidbody2D>().AddForce(-force.magnitude * collision.contacts[0].normal * 5);
        }
    }


    private void Update()
    {
        if (Energy <= 0)
        {
            Destroy(gameObject);
        }
        //transform.localScale += Vector3.one * GrowSpeed * Time.deltaTime;
        //GetComponent<Renderer>().material.SetFloat("_Thickness", Mathf.Lerp(0, 0.95f, Mathf.Sqrt(transform.localScale.x)));
    }

}