using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bacteria : MonoBehaviour
{
    public const int MinVertexCount = 6;
    public const float RadiusPerVertex = 0.018f;
    private const float randomOffsetSize = 0.25f;
    public enum CollisionType
    {
        None = 0,
        UnConnected,
        All
    }

    public float Health { get { return _health; } set { _health = value; } }

    public float Radius
    {
        get { return RadiusPerVertex * _vertices ; }
    }
    public float MaxSize
    {
        get
        { 
            return _maxSize;
        }
    }

    public int Vertices
    {
        get { return _vertices; }
        set
        {            
            if (_initialized && _vertices != value)
            {
                _vertices = value;
                UpdateVerticies();
            }
            _vertices = value;
        }
    }

    public CollisionType Collisions
    {
        get { return _collisionType; }
        set
        {            
            if (_initialized && _collisionType != value)
            {
                _collisionType = value;
                UpdateCollisions(_collisionType);
            }
            _collisionType = value;
        }
    }

    public Node this[int index] 
    {
        get  { return _nodes[index]; }
        set  { _nodes[index] = value; }
    }

    [SerializeField]
    private Node _node;
    [SerializeField]
    private int _vertices;
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
    private CollisionType _collisionType = CollisionType.None;
    private bool _initialized = false;

    private float _angle = 0;
    private float _initialOffset = 0;
    private float _maxSize = 0;
    private void Start()
    {
        _drawer = GetComponent<BacteriaDrawer>();
        _ai = GetComponent<BacteriaAI>();
        Generate();
    }
          
    public void Generate()
    {        
        if (_vertices < MinVertexCount)
        {
            return;
        }
        _center = GetComponent<Node>();
        _center.Collider.radius = Radius;
        _center.Collider.enabled = false;
        _angle = (2 * Mathf.PI) / _vertices;
        _initialOffset = Random.Range(0, Mathf.PI);

        Vector3[] outerPoints = new Vector3[_vertices];
        for (int i = 0; i < _vertices; i++)
        {
            _nodes.Add(Instantiate(_node.gameObject).GetComponent<Node>());
            float randomOffset = Random.Range(-_angle, _angle) * randomOffsetSize;
            float angleOffset = _initialOffset - i * _angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * Radius * Random.Range(_lowerRandomBound, _upperRandomBound);


            _nodes[i].transform.SetParent(transform, true);
            _nodes[i].Body.position = transform.TransformPoint(position);
            _nodes[i].Collider.radius = Radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].OnCollisionEnter += OnBacteriaHit;

            outerPoints[i] = position + position.normalized * _nodes[i].Collider.radius;
        }
        _maxSize = outerPoints[0].magnitude;
        foreach (var point in outerPoints)
        {
            _maxSize = Mathf.Max(_maxSize, point.magnitude);
        }
        _maxSize *= 2;
        Reconnect();
        UpdateCollisions(_collisionType);
        _initialized = true;
        _drawer.Init(this);
        _ai.Init(this);    

        if (GetComponent<Wrappable>() != null)
        {
            GetComponent<Wrappable>().Size = _maxSize;
        }

    }
    private void OnBacteriaHit(Collision2D collision,Node node)
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

    public bool ContainsNode(Node node)
    {
        return _nodes.Contains(node);
    }

    #region realtime update

    public void Regenerate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].transform.parent = null;
            foreach (var joint in _nodes[i].GetComponentsInChildren<Joint2D>())
            {
                DestroyImmediate(joint);
            }
            DestroyImmediate(_nodes[i].gameObject);
        }
        _nodes.Clear();

        Generate();
    }

    private void UpdateRadius(float radius)
    {
        _center.Collider.radius = radius;
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].Collider.radius = radius  * Random.Range(_lowerRandomBound, _upperRandomBound);;
            _nodes[i].Body.position = transform.TransformPoint(_nodes[i].transform.localPosition.normalized * radius  * Random.Range(_lowerRandomBound, _upperRandomBound));

            if (_collisionType != CollisionType.None)
            {
                _nodes[i].Collider.radius = Mathf.Min(
                    Vector3.Distance(_nodes[i].Body.position, transform.position),
                    Vector3.Distance(_nodes[i].Body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].Body.position),
                    Vector3.Distance(_nodes[i].Body.position, _nodes[(i + 1) % _nodes.Count].Body.position)
                );
            }
        }

        Reconnect();
    }

    private void UpdateCollisions(CollisionType type)
    {
        bool ignore = type == CollisionType.None;
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = 0; j < _nodes.Count; j++)
            {
                if (i != j)
                {
                    Physics2D.IgnoreCollision(_nodes[i].Collider, _nodes[j].Collider, ignore);
                }
            }
            if (!ignore)
            {
                _nodes[i].Collider.radius = Mathf.Min(
                    Vector3.Distance(_nodes[i].Body.position, transform.position),
                    Vector3.Distance(_nodes[i].Body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].Body.position),
                    Vector3.Distance(_nodes[i].Body.position, _nodes[(i + 1) % _nodes.Count].Body.position)
                );
            }
        }

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (Node.JointType nodetype = Node.JointType.Center; nodetype < Node.JointType.TypeLength; nodetype++)
            {
                _nodes[i][nodetype].Joint.enableCollision = type == CollisionType.All;

            }
        } 
        Reconnect();
    }

    private void Reconnect()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].ConnectSpring(Node.JointType.Left, _nodes[i == 0 ? _nodes.Count - 1 : i - 1]);
            _nodes[i].ConnectSpring(Node.JointType.Right, _nodes[(i + 1) % _nodes.Count]);
            _nodes[i].ConnectSpring(Node.JointType.Center, _center);
            _nodes[i].ConnectSlider(Node.JointType.Center, _center);

        }
    }

    private void UpdateVerticies()
    {
        if (_nodes.Count < _vertices)
        {
            for (int i = _nodes.Count; i < _vertices; i++)
            {
                _nodes.Add(Instantiate(_node.gameObject).GetComponent<Node>());    
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

        _angle = (2 * Mathf.PI) / _vertices;
        _initialOffset = Random.Range(0, Mathf.PI);
        for (int i = 0; i < _vertices; i++)
        {            
            float randomOffset = Random.Range(-_angle, _angle) * randomOffsetSize;
            float angleOffset = _initialOffset - i * _angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * Radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].Collider.radius = Radius * Random.Range(_lowerRandomBound, _upperRandomBound);;
            _nodes[i].transform.SetParent(transform, true);
            _nodes[i].Body.position = transform.TransformPoint(position);
        }
        Reconnect();
        UpdateCollisions(_collisionType);
        _drawer.Init(this);
        _ai.Init(this);
    }
    #endregion


}
