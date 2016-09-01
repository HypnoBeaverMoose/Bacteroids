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

    private GameController _controller;
    public float Amount { get { return _amount; } }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().Consume(this);
            _controller.Score += Amount;
            Explode(10);
        }
        else if (collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponentInParent<Bacteria>().Consume(this);
            Explode(10);

        }
    }
}
