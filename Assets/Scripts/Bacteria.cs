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
        Energy = _startEnergy;

    }

    private void Start()
    {
        _softbody.Init();
        _rigidbody = GetComponent<Rigidbody2D>();

    }    

    public void OnCollisionEnterChild(Rigidbody2D child, Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            child.AddForceAtPosition(collision.relativeVelocity.magnitude * collision.contacts[0].normal, collision.contacts[0].point, ForceMode2D.Impulse);

            var go = Instantiate<GameObject>(_energy);
            float damage = Random.Range(5, 20);
            this.Energy -= damage;
            go.GetComponent<Energy>().Amount = damage;
            go.transform.position = collision.contacts[0].point;
            go.GetComponent<Rigidbody2D>().AddForce(-collision.relativeVelocity.magnitude * collision.contacts[0].normal / 2, ForceMode2D.Impulse);
        }

        if (collision.gameObject.CompareTag("Player"))
        { 
            collision.rigidbody.AddForceAtPosition(3 * Random.insideUnitCircle.normalized, collision.contacts[0].point, ForceMode2D.Impulse);
            collision.gameObject.GetComponent<Player>().Energy -= Random.Range(5, 20);
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