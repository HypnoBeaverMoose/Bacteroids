using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ExplosionController.ExplosionType _explosion;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _velocityLimit;
    [SerializeField]
    private SpriteRenderer _sprite;
    [SerializeField]
    private float _damage;
    [SerializeField]
    private float _colorPenalty;
    [SerializeField]
    private int _emitRate;
    [SerializeField]
    private ParticleSystem _trail;

    private Rigidbody2D _rigidbody;
    private Color _color;
    private Color _tmpColor;

    public float Damage { get { return _damage; } }

    public Color Color { get { return _color; }
        set
        {
            _color = value;
            _sprite.color = new Color(_color.r, _color.g, _color.b, _sprite.color.a);
            _tmpColor = _sprite.color;
        }
    }

	void Start () 
    {
        _color = _sprite.color;
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.AddForce(transform.up * _speed);

    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        float sqrVelocity = _rigidbody.velocity.sqrMagnitude;
        if (sqrVelocity > 0.5f && sqrVelocity < _velocityLimit)
        {
            Kill(false);
        }

        _tmpColor.a = Mathf.Clamp01(1.0f - _velocityLimit / sqrVelocity);        
        _tmpColor.a *= _tmpColor.a;
        _sprite.color = _tmpColor;
        _trail.Emit((int)(_emitRate * _tmpColor.a));
    }

    public void Kill(bool explode = true)
    {
        if (explode)
        {
            ExplosionController.Instance.SpawnExplosion(_explosion, transform.position, Color);
        }
        _trail.Stop();
        _trail.transform.SetParent(null);
        Destroy(_trail.gameObject, 5);
        Destroy(gameObject);
    }

    public float GetDamage(Color toColor)
    {
        return (Color == toColor || toColor == Color.white) ? _damage : _damage * _colorPenalty;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Kill();
        GetComponent<Collider2D>().enabled = false;
    }
}
