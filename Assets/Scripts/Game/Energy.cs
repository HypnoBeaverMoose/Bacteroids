using UnityEngine;
using System.Collections;

public class Energy : MonoBehaviour 
{

    [SerializeField]
    private float _amount;
    [SerializeField]
    private ParticleSystem _explosion;
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private float _spawnBacteriaTimeout;
    [SerializeField]
    private float _radiusChange;

    public float RadiusChange { get { return _radiusChange; } set { _radiusChange = value; } }

    private GameController _controller;
    public float Amount { get { return _radiusChange * 1000; } }
    public Color Color { get { return _renderer.color; } set { _renderer.color = value; } }

	private void Start () 
    {
        GetComponent<Wrappable>().Size = transform.localScale.x;
        _controller = FindObjectOfType<GameController>();
    }

    private void Explode(int particleAmount)
    {
        var exp = Instantiate(_explosion);
        exp.startColor = Color;
        exp.transform.position = transform.position;
        exp.Emit(particleAmount);
        Destroy(exp.gameObject, 5);
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && Vector2.Distance(collider.transform.position, transform.position) < 0.5f)
        {
            collider.GetComponent<Player>().Consume(this);
            _controller.Score += Amount;
            Explode(10);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Explode(10);
        }
    }
}
