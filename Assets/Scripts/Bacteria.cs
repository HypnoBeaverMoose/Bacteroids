using UnityEngine;
using System.Collections;

public class Bacteria : MonoBehaviour {

    public static float MIN_SIZE = 0.6f;
    // Use this for initialization
    //[SerializeField]
    //private ParticleSystem _explosion;
    private float _size;
    private BoxCollider2D _collider; 
    private Rigidbody2D _rigidbody;
    private int _speed = 1;
    private bool _destructionStarted = false;
	
    void Start () 
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        //_organs.Stop();
        //GetComponent<Rigidbody2D>().velocity = MoveSpeed;
        //GetComponent<Rigidbody2D>().angularVelocity = RotationSpeed;
        transform.localScale *= Random.Range(0.8f, 2.0f);
        _size = transform.localScale.x;
 
	}	

	void Update () 
    {
        //if (transform.localScale.x < MaxSize)
        //{
        //    //_size += GrowthSpeed * Time.smoothDeltaTime;
        //}
        transform.localScale = Vector3.one * 0.9f * _size +
                            Vector3.one * 0.1f * Mathf.Sin(Time.unscaledTime * 5);              
	}
    void FixedUpdate()
    {
        if(_rigidbody.velocity.magnitude < 0.5f)
            _rigidbody.AddForce(Random.insideUnitCircle * _speed, ForceMode2D.Impulse);

        if (Mathf.Abs(_rigidbody.angularVelocity) < 5)
            _rigidbody.AddTorque(Random.Range(-10, 10));

    }
    public void Split(Vector2 tangent)
    {
        var newSize = transform.localScale * 0.5f;
        var perp = new Vector2(tangent.y, -tangent.x);
        if (newSize.x > MIN_SIZE)
        {
            var newBact = (GameObject)Instantiate(gameObject,  transform.position + (Vector3)(newSize.x * perp * 0.5f), Quaternion.identity);
            newBact.GetComponent<Rigidbody2D>().AddForce(perp * _speed, ForceMode2D.Impulse);
            newBact.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-10, 10));
            newBact.transform.localScale = newSize;
            newBact.tag = gameObject.tag;
            newBact = (GameObject)Instantiate(gameObject, transform.position - (Vector3)(newSize.x * perp * 0.5f), Quaternion.identity);
            newBact.GetComponent<Rigidbody2D>().AddForce(-perp * _speed, ForceMode2D.Impulse);
            newBact.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-10, 10));
            newBact.transform.localScale = newSize;
            newBact.tag = gameObject.tag;
        }
        Kill();

    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("Ignored");
            Physics2D.IgnoreCollision(collider, _collider);
            Split(collider.GetComponent<Rigidbody2D>().velocity.normalized);
            Destroy(collider.gameObject);
        }
        else if (collider.gameObject.CompareTag("Player"))
        {
            if (transform.localScale.x <= MIN_SIZE)
            {
                collider.GetComponent<Player>().Energy += 10;
                Kill();
            }
            else if (collider.GetComponent<Player>() != null)
            {                
                collider.GetComponent<Player>().Energy -= 20;
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("MainCamera"))
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0;
            _rigidbody.AddForce(Random.insideUnitCircle * _speed, ForceMode2D.Impulse);
            _rigidbody.AddTorque(Random.Range(-10, 10));
        }
    }

    public void Kill()
    {
        if (!_destructionStarted)
            StartCoroutine(DestroyCoroutine());
    }
    private IEnumerator DestroyCoroutine()
    {
        _destructionStarted = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        var explosion = Instantiate(Resources.Load<ParticleSystem>("explosion"));
        explosion.transform.SetParent(transform,false);
        explosion.Emit(30);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
    
}
