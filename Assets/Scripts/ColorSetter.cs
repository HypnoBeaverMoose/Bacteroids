using UnityEngine;
using System.Collections;

public class ColorSetter : MonoBehaviour {


    public Color Color { get; set; }
    [SerializeField]
    private float _growSpeed;
    [SerializeField]
    private float _maxSize;
    [SerializeField]
    private float _force;

    private float _size = 1.0f;
    private Vector3 _scale;
    private Vector3 _velocity;
    void Start () 
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(Color.r, Color.g, Color.b, Color.a);
        _scale = transform.localScale;
        
	}
	
	void Update () 
    {
        //_size += _growSpeed * Time.deltaTime;
        transform.localScale = Vector3.SmoothDamp(transform.localScale, _scale * _maxSize, ref _velocity, 0.2f);
        
        if (Vector3.Distance(transform.localScale, _scale * _maxSize) < 0.1f)
        {
            Destroy(gameObject);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bacteria"))
        {
            if (other.GetComponent<Bacteria>() != null)
            {
                other.GetComponent<Bacteria>().Color = Color;
            }
            else if (other.GetComponent<Rigidbody2D>() != null)
            {
                other.GetComponent<Rigidbody2D>().AddForce((other.transform.position - transform.position).normalized, ForceMode2D.Impulse);
            }
        }
    }
}
