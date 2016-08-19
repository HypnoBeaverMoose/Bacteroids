using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaAI : MonoBehaviour
{

    [SerializeField]
    private float _blobIntensity;

    [SerializeField]
    private float _blobSpeed;

    [SerializeField]
    private float _moveTimeot;
    [SerializeField]
    private float _moveForce;


    private Player _player;
    private Bacteria _bacteria;
    private Vector3 _direction;

    public Vector3 Direction { get { return _direction; } set { _direction = value; } }

    void Start()
    {
        _direction = Random.insideUnitCircle.normalized;
        InvokeRepeating("Move", 0, _moveTimeot);
        StartCoroutine(Blob());
    }

    public void Init(Bacteria bacteria)
    {
        _bacteria = bacteria;
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            _bacteria[i].OnCollisionEnter += NodeCollision;
        }
    }

    public void Clear()
    {
        StopAllCoroutines();
    }

    private IEnumerator Blob()
    {        
        while (true && _bacteria != null)
        {
            var body = _bacteria[Random.Range(0, _bacteria.Vertices)].Body;
            body.AddForce((body.position - (Vector2)transform.position).normalized * Random.Range(0.01f, 0.1f) *_blobIntensity, ForceMode2D.Impulse);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f) * _blobSpeed);
        }
    }

    private void NodeCollision(Collision2D collision, Node node)
    {
        _direction = Random.insideUnitCircle;
        Vector2 normal = ((Vector2)collision.transform.position - node.Body.position).normalized;
        if (Vector2.Dot(_direction, normal) > 0)
        {
            _direction *= -1;
        }

        if (collision.collider.CompareTag("Energy"))
        {
            node.Body.AddForce(normal * 5, ForceMode2D.Impulse);
        }
    }

    private void Move()
    {        
        _bacteria.GetComponent<Node>().Body.AddForce(_direction * _moveForce, ForceMode2D.Impulse);
    }
}
