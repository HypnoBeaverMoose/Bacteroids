using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaAI : MonoBehaviour
{

    public bool NearPlayer { get { return _nodesNearPlayer > 0; } }


    [SerializeField]
    private float _blobIntensity;

    [SerializeField]
    private float _blobSpeed;

    [SerializeField]
    private float _moveTimeot;
    [SerializeField]
    private float _moveForce;

    private int _nodesNearPlayer = 0;
    private Player _player;
    private Bacteria _bacteria;

    void Start()
    {
        //InvokeRepeating("Move", _moveTimeot, _moveTimeot);
        StartCoroutine(Blob());
    }

    public void Init(Bacteria bacteria)
    {
        _bacteria = bacteria;
    }

    private void NodeNearPlayer(Collider2D other, Node node)
    {
        if (_player == null)
        {
            _player = other.GetComponent<Player>();
        }
        _nodesNearPlayer++;
    }

    private void NodeAwayFromPlayer(Collider2D other, Node node)
    {
        _nodesNearPlayer--;
        if (_nodesNearPlayer < 0)
        {
            Debug.LogError("Nodes Near Player is Negative, Setting to 0");
            _nodesNearPlayer = 0;
        }

    }

    private IEnumerator Blob()
    {
        while (true)
        {
            var body = _bacteria[Random.Range(0, _bacteria.Vertices)].Body;
            body.AddForce((body.position - (Vector2)transform.position).normalized * Random.Range(0.01f, 0.1f) *_blobIntensity, ForceMode2D.Impulse);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f) * _blobSpeed);
        }
    }

    private void Move()
    {
//        Vector3 direction = Random.insideUnitCircle.normalized;
//        float mult = 1;
//        if (NearPlayer && _player != null)
//        {
//            mult = 2;
//            direction = (_player.transform.position - transform.position).normalized;
//        }
//        int maxIndex = 0;
//        float maxValue = -1;
//        for (int i = 0; i < _nodes.Count; i++)
//        {
//            float dot = Vector3.Dot((_nodes[i].transform.position - transform.position).normalized, direction);
//            if (dot > maxValue)
//            {
//                maxValue = dot;
//                maxIndex = i;
//            }
//        }
//        _nodes[maxIndex].Body.AddForce(direction * _moveForce * mult, ForceMode2D.Impulse);
    }
}
