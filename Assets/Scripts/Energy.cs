using UnityEngine;
using System.Collections;

public class Energy : MonoBehaviour 
{

    [SerializeField]
    private ParticleSystem _explosion;
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private float _spawnBacteriaTimeout;

    public float Amount { get; set; }
    public Color Color { get { return _renderer.color; } set { _renderer.color = value; } }

	private void Start () 
    {
        Invoke("SpawnBacteria", _spawnBacteriaTimeout);
    }

    private void FixedUpdate()
    {
        //transform.localScale = Vector3.one * 0.95f * _size +
        //              Vector3.one * 0.05f * Mathf.Sin(Time.unscaledTime * 5);              

    }

    private void Explode(int particleAmount)
    {
        var exp = Instantiate(_explosion);
        exp.startColor = Color;
        exp.transform.position = transform.position;
        exp.Emit(particleAmount);
        Destroy(exp.gameObject, 5);
    }

    private void SpawnBacteria()
    {
        if (gameObject != null && Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Bacteria")) == null)
        {            
            FindObjectOfType<GameController>().SpawnBacteria(transform.position, 0.1f, 5, Color);
            Explode(50);            
            DestroyImmediate(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Explode(10);
            Destroy(gameObject);
        }
    }
}
