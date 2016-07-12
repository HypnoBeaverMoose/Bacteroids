using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaAI : MonoBehaviour 
{

    public bool NearPlayer { get { return _nodesNearPlayer > 0; } }    
    
    [SerializeField]
    private float _moveTimeot;
    [SerializeField]
    private float _moveForce;

    private List<Node> _nodes;
    private int _nodesNearPlayer = 0;
    private Player _player;

	void Start () 
    {
        InvokeRepeating("Move", _moveTimeot, _moveTimeot);
	}

    public void Init(List<Node> nodes)
    {
        _nodes = new List<Node>(nodes);
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
