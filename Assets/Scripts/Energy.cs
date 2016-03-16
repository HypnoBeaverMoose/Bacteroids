using UnityEngine;
using System.Collections;

public class Energy : MonoBehaviour 
{

    [SerializeField]
    private float _sizeFactor;

    public float Amount { get; set; }

    private float _size;
	void Start () 
    {
        //Amount = 20;
        _size = Amount * _sizeFactor;
	}

    void FixedUpdate()
    {
        transform.localScale = Vector3.one * 0.9f * _size +
                      Vector3.one * 0.1f * Mathf.Sin(Time.unscaledTime * 5);              

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().Energy += Amount;
            Destroy(gameObject);
        }
    }
}
