using UnityEngine;
using System.Collections;

public class CollisionHandler : MonoBehaviour 
{

    private Bacteria _bacteria;

	private void Start () 
    {
        _bacteria = GetComponentInParent<Bacteria>();
	}


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_bacteria != null)
        {
            _bacteria.OnCollisionEnterChild(GetComponent<Rigidbody2D>(), collision);
        }
    }

}
