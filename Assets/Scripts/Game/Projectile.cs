using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ExplosionController.ExplosionType _explosion;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private SpriteRenderer _sprite;
    [SerializeField]
    private float _radiusChange;
    [SerializeField]
    private float _damage;
    [SerializeField]
    private float _colorPenalty;

    public float RadiusChange { get { return _radiusChange; } }

    public Color Color { get { return _sprite.color; } set { _sprite.color = value; } }

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
        ExplosionController.Instance.SpawnExplosion(_explosion, transform.position, Color);
        Destroy(gameObject);
    }

    public float GetDamage(Color toColor)
    {
        return (Color == toColor || toColor == Color.white) ? _damage : _damage * _colorPenalty;
    }
    
    private void DestroyCoroutine()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Kill();
        GetComponent<Collider2D>().enabled = false;
    }
}
