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
    private float _growTimeout;
    [SerializeField]
    private float _moveTimeot;
    [SerializeField]
    private float _moveForce;


    private Player _player;
    private Bacteria _bacteria;
    private Vector3 _direction;

    public float MoveTimeout { get { return _moveTimeot;  } set { _moveTimeot = value; } }
    public Vector3 Direction { get { return _direction; } set { _direction = value; } }

    void Start()
    {
        _direction = Random.insideUnitCircle.normalized;
        StartCoroutine(Blob());
        InvokeRepeating("Move", Random.Range(0.2f, 1.5f), _moveTimeot);
        InvokeRepeating("Grow", Random.Range(0.2f, 1.5f), _growTimeout);
        InvokeRepeating("ChangeDirection", 10, 10);
        
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
        CancelInvoke("Grow");
        CancelInvoke("Move");
        CancelInvoke("ChangeDirection");
    }

    private IEnumerator Blob()
    {        
        while (_bacteria != null)
        {
            var body = _bacteria[Random.Range(0, _bacteria.Vertices)].Body;
            body.AddForce((body.position - (Vector2)transform.position).normalized * Random.Range(0.01f, 0.1f) *_blobIntensity, ForceMode2D.Impulse);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f) * _blobSpeed);
        }
    }

    private void NodeCollision(Collision2D collision, Node node)
    {
        Vector2 normal = (collision.contacts[0].point - (Vector2)_bacteria.transform.position).normalized;
        if (collision.collider.CompareTag("Projectile"))
        {
            _direction = new Vector3(-normal.y, normal.x, 0).normalized;
            _bacteria.GetComponent<Rigidbody2D>().AddForce(_direction * _moveForce * 2, ForceMode2D.Impulse);
            FindNodeForDirection(_direction).Body.AddForce(_direction * _moveForce * 2, ForceMode2D.Impulse);
        }

        if (collision.collider.CompareTag("Energy"))
        {
            node.Body.AddForce(normal * 0.5f, ForceMode2D.Impulse);
        }

    }
    private void ChangeDirection()
    {
        _direction = Random.insideUnitCircle.normalized;
    }
    private void Move()
    {
        if (_bacteria != null)
        {            
            FindNodeForDirection(_direction).Body.AddForce(_direction * _moveForce, ForceMode2D.Impulse);
        }
    }

    private void Grow()
    {
        if (_bacteria == null)
        {
            return;
        }

        if (_bacteria.Radius > _bacteria.Growth.MaxRadius)
        {
            if (GameController.Instance.Spawn.CanSpawn)
            {
                Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.BacteriaSplitAlone, transform.position);
                GameController.Instance.Spawn.Split(_bacteria, 0, Indexer.GetIndex(Indexer.IndexType.Across, 0, _bacteria.Vertices));
            }
        }
        else
        {
            _bacteria.Radius += _bacteria.Growth.GrowthRate * _growTimeout;
        }
    }

    private Node FindNodeForDirection(Vector2 direction)
    {
        float angle = Vector2.Dot(direction, _bacteria[0].transform.localPosition.normalized);
        int index = 0;
        for (int i = 1; i < _bacteria.Vertices; i++)
        {
            float newAngle = Vector2.Dot(direction, _bacteria[i].transform.localPosition.normalized);
            if (newAngle > angle)
            {
                angle = newAngle;
                index = i;
            }
        }
        return _bacteria[index];
    }

}