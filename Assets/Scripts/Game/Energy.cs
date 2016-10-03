using UnityEngine;
using System.Collections;

public class Energy : MonoBehaviour 
{

    [SerializeField]
    private ExplosionController.ExplosionType _explosion;
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private float _radiusChange;
    [SerializeField]
    private float _consumableDistance;
    [SerializeField]
    private int _explosionParticles;

    public float RadiusChange { get { return _radiusChange; }
        set
        {
            transform.localScale *= Mathf.Sqrt(value / _radiusChange);
            _radiusChange = value;
        }
    }

    private GameController _controller;
    public int Score { get { return (int)(_radiusChange * 1000); } }
    public Color Color { get { return _renderer.color; } set { _renderer.color = value; } }

	private void Start () 
    {
        GetComponent<Wrappable>().Size = transform.localScale.x;
        _controller = FindObjectOfType<GameController>();
    }

    private void Kill()
    {
        ExplosionController.Instance.SpawnExplosion(_explosion, transform.position, Color);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && Vector2.Distance(collider.transform.position, transform.position) < _consumableDistance)
        {
            collider.GetComponent<Player>().Consume(this);
            Kill();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Kill();
        }
    }
}
