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
    private GameObject _scoreDisplay;


    public float RadiusChange { get { return _radiusChange; }
        set
        {
            transform.localScale *= Mathf.Sqrt(Mathf.Sqrt(value / _radiusChange));
            _radiusChange = value;
        }
    }

    private GameController _controller;
    public int Score { get { return (int)(_radiusChange * 1000); } }
    public Color Color { get { return _renderer.color; } set { _renderer.color = value; } }

	private void Start () 
    {
        _controller = FindObjectOfType<GameController>();
        Invoke("SwitchLayer", 0.5f);
    }

    private void SwitchLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Energy");
    }

    public void Kill()
    {
        ExplosionController.Instance.SpawnExplosion(_explosion, transform.position, Color);
        var display = Instantiate<GameObject>(_scoreDisplay);
        display.transform.position = transform.position;
        display.GetComponentInChildren<UnityEngine.UI.Text>().text = Score.ToString();
        display.GetComponentInChildren<UnityEngine.UI.Text>().color = Color;
        display.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle.normalized * 3);
        Destroy(display, 1);
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
        else if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().Consume(this);
            Kill();

        }
    }
}
