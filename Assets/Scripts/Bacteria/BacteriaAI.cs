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

    public Vector3 Direction { get { return _direction; } set { _direction = value; } }

    void Start()
    {
        _direction = Random.insideUnitCircle.normalized;
        StartCoroutine(Blob());
        InvokeRepeating("Move", _moveTimeot * Random.Range(0.2f, 1.5f), _moveTimeot);
        InvokeRepeating("Grow", _growTimeout * Random.Range(0.2f, 1.5f), _growTimeout);

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
        _direction = Random.insideUnitCircle;
        Vector2 normal = ((Vector2)collision.transform.position - node.Body.position).normalized;
        if (Vector2.Dot(_direction, normal) > 0)
        {
            _direction *= -1;
        }

        if (collision.collider.CompareTag("Energy"))
        {
            node.Body.AddForce(normal * 0.5f, ForceMode2D.Impulse);
        }
    }

    private void Move()
    {
        if (_bacteria != null)
        {
            _bacteria[Random.Range(0, _bacteria.Vertices)].Body.AddForce(_direction * _moveForce, ForceMode2D.Impulse);
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
            if (GameController.Instance.Spawn.CanSplit())
            {
                GameController.Instance.Spawn.Split(_bacteria, 0, Indexer.GetIndex(Indexer.IndexType.Across, 0, _bacteria.Vertices));
            }
        }
        else
        {
            _bacteria.Radius += _bacteria.Growth.GrowthRate * _growTimeout;
        }
    }
}
