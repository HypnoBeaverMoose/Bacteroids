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

    public Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            if (_initialized)
            {
                _drawer.Color = _color;
            }
        }
    }

    public Node[] GetNodes()
    {
        return _nodes.ToArray();
    }

    public void SetNodes(List<Node> nodes)
    {
        _nodes.AddRange(nodes);
        Vertices = _nodes.Count;
    }

    public bool Contains(Node node)
    {
        return _nodes.Contains(node);
    }

    public BacteriaGrowth Growth
    {
        get { return _growth; }
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
    [SerializeField]
    private Color _color;

    private Node _center;
    private BacteriaDrawer _drawer;
    private BacteriaMutate _mutate;
    private BacteriaGrowth _growth;
    private BacteriaAI _ai;
    private List<Node> _nodes = new List<Node>();
    private Collider2D _lastHit = null;
    private bool _initialized = false;
    private float _angle = 0;
    private float _initialOffset = 0;

    private void Start()
    {
        _drawer = GetComponent<BacteriaDrawer>();
        _ai = GetComponent<BacteriaAI>();
        _mutate = GetComponent<BacteriaMutate>();
        _growth = GetComponent<BacteriaGrowth>();
        _center = GetComponent<Node>();
        _center.Collider.radius = _radius;
        _initialOffset = Random.Range(0, Mathf.PI);
        UpdateVerticies();
        _drawer.Color = _color;
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

        for (int i = 0; i < _vertices; i++)
        {
            float randomOffset = Random.Range(-_angle, _angle) * randomOffsetSize;
            float angleOffset = _initialOffset - i * _angle + randomOffset;
            _nodes[i].gameObject.SetActive(true);
            _nodes[i].transform.SetParent(transform, true);
            _nodes[i].TargetBody = _center.Body;
            _nodes[i].TargetPosition = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].Collider.radius = _radius * Random.Range(_lowerRandomBound, _upperRandomBound);
            _nodes[i].ClearEvents();
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

    public void UpdateVerticies()
    {
        if (_nodes.Count < _vertices)
        {
            for (int i = _nodes.Count; i < _vertices; i++)
            {
                var node = Instantiate(_node.gameObject).GetComponent<Node>();
                node.transform.position = transform.position;
                _nodes.Insert(Random.Range(0, _nodes.Count), node);

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
        _mutate.Init(this);

        if (GetComponent<Wrappable>() != null)
        {
            GetComponent<Wrappable>().Size = _radius * 4;
        }
    }

    private void UpdateRadius()
    {
        if (_center.Collider != null)
        {
            _center.Collider.radius = _radius;
        }
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
        if (_lastHit != null && _lastHit == collision.collider)
        {
            return;
        }
        _lastHit = collision.collider;


        if (collision.collider.CompareTag("Projectile"))
        {
            Hit(collision.gameObject.GetComponent<Projectile>(), collision.contacts[0].point, collision.relativeVelocity, node);
        }
        else if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().Damage(this);
        }
        else if (collision.collider.CompareTag("Energy"))
        {
            Consume(collision.gameObject.GetComponent<Energy>());
        }
    }

    public void Hit(Projectile projectile, Vector2 hit, Vector2 velocity, Node node)
    {
        float damage = projectile.GetDamage(Color);
        node.Health -= damage;

        if (node.Health > 0)
        {
            return;
        }

        float decrease = damage * _growth.DecreaseRate;
        node.Health = 1.0f;
        float radius = Radius > _growth.MinRadius ? decrease : Radius;
        float energyPieces = Random.Range(1, 4);
        for (int i = 0; i < energyPieces; i++)
        {
            GameController.Instance.Spawn.SpawnEnergy(
                                    hit,
                                    (Random.insideUnitCircle + velocity.normalized).normalized,
                                    radius / energyPieces, Color);
        }
                
        if (Radius >= _growth.MinRadius * _growth.SplitWhenHitRatio)
        {
            int index = Mathf.Max(_nodes.IndexOf(node), 0);
            GameController.Instance.Spawn.Split(this, index, Indexer.GetIndex(Indexer.IndexType.Across, index, Vertices));
        }
        else  if (Radius > _growth.MinRadius)
        {
            Radius -= decrease;
        }
        else
        {
            _ai.Clear();
            _drawer.Clear();
            Destroy(gameObject);
        }
    }

    public void KillNode(int index)
    {
        if (index >= 0)
        {
            var node = _nodes[index];
            Vector2 vel = node.Body.velocity;
            node.Disconnect();
            node.ClearEvents();
            node.TargetBody = null;
            node.transform.SetParent(null, true);           
            _nodes.RemoveAt(index);
            Destroy(node.gameObject);
            Vertices = _nodes.Count;
            _nodes[((index >= _nodes.Count) ? _nodes.Count - 1 : index)].Body.velocity = vel;
        }
    }

    public void Consume(Energy Energy)
    {
        Radius += Energy.RadiusChange;
    }

    public void Clear()
    {
        _drawer.Clear();
        _ai.Clear();
        _mutate.Clear();

    }

    private void OnDestroy()
    {
        Clear();
    }
}