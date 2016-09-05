using System.Collections.Generic;
using UnityEngine;

public class Bacteria : MonoBehaviour
{
    [System.Serializable]
    public sealed class NodeConnection
    {
        public Indexer.IndexType IndexType;
        public float Frequency;
        public float Damping;
        public bool AutoDistance;
    }

    public NodeConnection[] nodeConnections;


    public const int MinVertexCount = 6;
    private const float randomOffsetSize = 0.25f;

    public float Health { get { return _health; } set { _health = value; } }

    public float Radius
    {
        get { return _radius; }
        set
        {
            bool needsUpdate = _initialized && _radius != value;
            _radius = value;
            if (needsUpdate)
            {
                UpdateRadius();
            }
        }
    }

    public int Vertices
    {
        get { return _vertices; }
        set
        {
            bool needsUpdate = _initialized && _vertices != value;
            _vertices = value;
            if (needsUpdate)
            {
                UpdateVerticies();
            }
        }
    }
    public Vector2 CenterOfMass
    {
        get
        {
            if (_nodes.Count == 0)
            {
                return _center.Body.position;
            }
            else
            {
                Vector2 com = Vector2.zero;
                foreach (var node in _nodes)
                {
                    com += node.Body.position;
                }
                return com / _nodes.Count;
            }
        }
    }


    public Node this[int index]
    {
        get { return _nodes[index]; }
        set { _nodes[index] = value; }
    }

    public Node[] Nodes { get { return _nodes.ToArray(); } }


    public bool Contains(Node node)
    {
        return _nodes.Contains(node);
    }


    [SerializeField]
    private Node _node;
    [SerializeField]
    private int _vertices;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _health;
    [SerializeField]
    private float _lowerRandomBound;
    [SerializeField]
    private float _upperRandomBound;

    private Node _center;
    private BacteriaDrawer _drawer;
    private BacteriaAI _ai;
    private List<Node> _nodes = new List<Node>();

    private bool _initialized = false;
    private float _angle = 0;
    private float _initialOffset = 0;

    private void Start()
    {
        _drawer = GetComponent<BacteriaDrawer>();
        _ai = GetComponent<BacteriaAI>();
        _center = GetComponent<Node>();
        _center.Collider.radius = _radius;

        UpdateVerticies();
    }

    public void Regenerate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].transform.parent = null;
            _nodes[i].OnCollisionEnter -= OnBacteriaHit;
            foreach (var joint in _nodes[i].GetComponentsInChildren<Joint2D>())
            {
                DestroyImmediate(joint);
            }
            DestroyImmediate(_nodes[i].gameObject);
        }
        _nodes.Clear();
        UpdateVerticies();
    }

    private void UpdateNodes()
    {
        _angle = (2 * Mathf.PI) / _vertices;
        _initialOffset = Random.Range(0, Mathf.PI);

        for (int i = 0; i < _vertices; i++)
        {
            float randomOffset = Random.Range(-_angle, _angle) * randomOffsetSize;
            float angleOffset = _initialOffset - i * _angle + randomOffset;

            _nodes[i].transform.SetParent(transform, true);
            _nodes[i].TargetBody = _center.Body;
            _nodes[i].TargetPosition = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].Collider.radius = _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].OnCollisionEnter += OnBacteriaHit;
        }
    }

    private void UpdateCollisions()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            Physics2D.IgnoreCollision(node.Collider, _center.Collider);
            for (int j = 0; j < _nodes.Count; j++)
            {
                if (i != j)
                {
                    Physics2D.IgnoreCollision(node.Collider, _nodes[j].Collider, true);
                }
            }
        }
    }

    private void UpdateVerticies()
    {
        if (_vertices < MinVertexCount)
        {
            return;
        }

        if (_nodes.Count < _vertices)
        {
            for (int i = _nodes.Count; i < _vertices; i++)
            {
                var node = Instantiate(_node.gameObject).GetComponent<Node>();
                node.transform.position = transform.position;
                _nodes.Add(node);

            }
        }
        else if (_nodes.Count > _vertices)
        {
            for (int i = _vertices; i < _nodes.Count; i++)
            {
                _nodes[i].transform.parent = null;
                foreach (var joint in _nodes[i].GetComponentsInChildren<Joint2D>())
                {
                    DestroyImmediate(joint);
                }
                DestroyImmediate(_nodes[i].gameObject);
            }
            _nodes.RemoveRange(_vertices, _nodes.Count - _vertices);
        }

        UpdateNodes();
        UpdateCollisions();
        Reconnect();
        _initialized = true;

        _drawer.Init(this);
        _ai.Init(this);

        if (GetComponent<Wrappable>() != null)
        {
            GetComponent<Wrappable>().Size = _radius * 4;
        }
    }

    private void UpdateRadius()
    {
        _center.Collider.radius = _radius;
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].Collider.radius = _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].TargetPosition = _nodes[i].TargetPosition.normalized * _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
        }
        Reconnect();
    }

    public void Reconnect()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].Disconnect();
            foreach (var connection in nodeConnections)
            {
                var other = _nodes[Indexer.GetIndex(connection.IndexType, i, _nodes.Count)];
                var spring = _nodes[i].ConnectSpring(other, Vector2.zero, connection.Frequency, connection.Damping) as SpringJoint2D;
                spring.distance = Vector2.Distance(_nodes[i].TargetPosition, other.TargetPosition);
            }
        }
    }

    private void OnBacteriaHit(Collision2D collision, Node node)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            GameController.Instance.Spawn.HandleHit(this, collision.gameObject.GetComponent<Projectile>(), collision.contacts[0].point, collision.relativeVelocity);
        }

        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().Damage(this);
        }
    }

    public void Consume(Energy Energy)
    {
    }

    private void OnDestroy()
    {
        _drawer.Clear();
        _ai.Clear();
        foreach (var node in _nodes)
        {
            node.OnCollisionEnter -= OnBacteriaHit;
        }
    }
}

//private CollisionType _collisionType = CollisionType.None;s
//    public enum CollisionType
//    {
//        None = 0,
//        UnConnected,
//        All
//    }
//
//    public CollisionType Collisions
//    {
//        get { return _collisionType; }
//        set
//        {
//            if (_initialized && _collisionType != value)
//            {
//                _collisionType = value;
//                UpdateCollisions(_collisionType);
//            }
//            _collisionType = value;
//        }
//    }
//    private void UpdateCollisions(CollisionType type)
//    {
//        bool ignore = type == CollisionType.None;
//            if (!ignore)
//            {
//                _nodes[i].Collider.radius = Mathf.Min(
//                    Vector3.Distance(_nodes[i].Body.position, transform.position),
//                    Vector3.Distance(_nodes[i].Body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].Body.position),
//                    Vector3.Distance(_nodes[i].Body.position, _nodes[(i + 1) % _nodes.Count].Body.position)
//                );
//            }
//        }
//
//        for (int i = 0; i < _nodes.Count; i++)
//        {
//            for (Node.JointType nodetype = Node.JointType.Center; nodetype < Node.JointType.TypeLength; nodetype++)
//            {
//                _nodes[i][nodetype].Joint.enableCollision = type == CollisionType.All;
//
//            }
//        }
//        Reconnect();
//    }
//private void UpdateVerticies()
//{


//    //_angle = (2 * Mathf.PI) / _vertices;
//    //_initialOffset = Random.Range(0, Mathf.PI);
//    //for (int i = 0; i < _vertices; i++)
//    //{
//    //    float randomOffset = Random.Range(-_angle, _angle) * randomOffsetSize;
//    //    float angleOffset = _initialOffset - i * _angle + randomOffset;
//    //    Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
//    //    _nodes[i].Collider.radius = _radius * Random.Range(_lowerRandomBound, _upperRandomBound);;
//    //    _nodes[i].transform.SetParent(transform, true);
//    //    _nodes[i].Body.position = transform.TransformPoint(position);
//    //}
//    UpdateNodes();
//    UpdateCollisions()
//    Reconnect();
//    _drawer.Init(this);
//    _ai.Init(this);
//}