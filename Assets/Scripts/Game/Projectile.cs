using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private GameObject _explosionPrefab;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private SpriteRenderer _sprite;
    [SerializeField]
    private float _radiusChange;

    public float RadiusChange { get { return _radiusChange; } }

    public Color Color { get { return _sprite.color; } set { _sprite.color = value; } }
    private bool _destructionStarted = false;

	void Start () 
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * _speed);
        
	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        if (!_sprite.isVisible)
        {
            Kill();
        }
	}

    public void Kill()
    {
        if (!_destructionStarted)
        {
            StartCoroutine(DestroyCoroutine());
        }
    }
    
    private IEnumerator DestroyCoroutine()
    {
        _destructionStarted = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        var explosion = Instantiate(_explosionPrefab);

        explosion.transform.position = transform.position;
        explosion.GetComponent<ParticleSystem>().startColor = Color;
        explosion.GetComponent<ParticleSystem>().Emit(30);
        Destroy(explosion.gameObject, 5);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
//        if (collision.rigidbody != null)
//        {
//            collision.rigidbody.AddForce( -collision.contacts[0].normal , ForceMode2D.Impulse);
//        }
        Kill();
    }
}
