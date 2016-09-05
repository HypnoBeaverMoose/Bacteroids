using UnityEngine;
using System.Collections;

public class Hammer : MonoBehaviour
{
    bool hit = false;
    public float force;
    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hit = true;
        }

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (hit)
        {
            var vector = (FindObjectOfType<Bacteria>().transform.position - transform.position).normalized;
            Physics2D.Raycast(transform.position, vector, 100, LayerMask.GetMask("Bacteria")).rigidbody.AddForce(vector * force, ForceMode2D.Force);
            Debug.DrawRay(transform.position, vector);
            hit = false;
        }
    }
}
