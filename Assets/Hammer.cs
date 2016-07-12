using UnityEngine;
using System.Collections;

public class Hammer : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var vector = (FindObjectOfType<Bacteria>().transform.position - transform.position).normalized;
            Physics2D.Raycast(transform.position, vector, 100, LayerMask.GetMask("Bacteria")).rigidbody.AddForce(vector * 100, ForceMode2D.Impulse);
            Debug.DrawRay(transform.position, vector);
        }

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
