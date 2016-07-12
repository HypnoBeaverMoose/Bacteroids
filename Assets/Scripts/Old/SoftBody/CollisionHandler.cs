using UnityEngine;
using System.Collections;

public class CollisionHandler : MonoBehaviour 
{

    private OldBacteria _bacteria;

	private void Start () 
    {
        _bacteria = GetComponentInParent<OldBacteria>();
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_bacteria != null)
        {
            _bacteria.OnCollisionEnterChild(GetComponent<Rigidbody2D>(), collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_bacteria != null)
        {
            _bacteria.OnTriggerEnterChild(GetComponent<Rigidbody2D>(), other);
        } 
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (_bacteria != null)
        {
            _bacteria.OnTriggerExitChild(GetComponent<Rigidbody2D>(), other);
        }
    }
}
