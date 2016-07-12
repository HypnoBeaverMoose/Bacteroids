using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bacteria : MonoBehaviour
{
    public float Radius { get { return _radius; } set { _radius = value; } }

    public float Health { get { return _health; } set { _health = value; } }

    public int Vertices { get { return _vertices; } set { _vertices = value; } }

    [SerializeField]
    private Node _node;
    [SerializeField]
    private int _vertices;
    [SerializeField]
    private float _health;
    [SerializeField]    
    private float _radius;

    private Node _center;
    private BacteriaDrawer _drawer;
    private BacteriaAI _ai;
    private List<Node> _nodes = new List<Node>();

    private void Start()
    {
        _drawer = GetComponent<BacteriaDrawer>();
        _ai = GetComponent<BacteriaAI>();
        _center = GetComponent<Node>();
        _center.Collider.radius = _radius;
        Generate();
        _drawer.Init(_nodes);
        _ai.Init(_nodes);
    }

    private void Generate()
    {
        float angle = (2 * Mathf.PI) / _vertices;
        float initialOffset = Random.Range(0, Mathf.PI);
        float randomOffsetSize = 0.25f;

        for (int i = 0; i < _vertices; i++)
        {
            _nodes.Add(Instantiate(_node.gameObject).GetComponent<Node>());
            float randomOffset = Random.Range(-angle, angle) * randomOffsetSize;
            float angleOffset = initialOffset - i * angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius;
            _nodes[i].transform.SetParent(transform, true);
            _nodes[i].Body.position = transform.TransformPoint(position);
        }

        for (int i = 0; i < _nodes.Count; i++)
        {
            Node node = _nodes[i];

            node.Connect(Node.JointType.Left, _nodes[i == 0 ? _nodes.Count - 1 : i - 1]);
            node.Connect(Node.JointType.Right, _nodes[(i + 1) % _nodes.Count]);
            node.Connect(Node.JointType.Center, _center);

            node.Collider.radius = _radius;
            node.Collider.radius = Mathf.Min(
                Vector3.Distance(node.Body.position, transform.position),
                Vector3.Distance(node.Body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].Body.position),
                Vector3.Distance(node.Body.position, _nodes[(i + 1) % _nodes.Count].Body.position)
            );
        }

//        for (int i = 0; i < _nodes.Count; i++)
//        {
//            for (int j = 0; j < _nodes.Count; j++)
//            {
//                if (i != j)
//                {
//                    Physics2D.IgnoreCollision(_nodes[i].Collider, _nodes[j].Collider);
//                }
//            }
//        }
    }

    //    public void TakeHit(Vector2 position, float damage)
    //    {
    //        Health -= damage;
    //        if (Health > 0)
    //        {
    //            FindObjectOfType<GameController>().SpawnSpore(position, 0.15f, Color.white);
    //        }
    //        else
    //        {
    //
    //        }
    //    }

    private void Update()
    {
    }
}
