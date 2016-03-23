using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundSoftBody : MonoBehaviour 
{
    

    [SerializeField]
    private Rigidbody2D _prototype;
    [SerializeField]
    private float _damping;
    [SerializeField]
    public float _frequency;

    
    public int Vertices;
    public float Size { get { return _radius * 2; } set { _radius = value / 2; } }
    
    
    private Mesh _mesh = null;

    private float _radius = 0;
    private List<Node> _nodes = new List<Node>();
    private Rigidbody2D _center = null;
    private Transform _transform = null;
    private List<Vector3>       _vertices = new List<Vector3>();
    private List<Vector3>       _normals = new List<Vector3>();
    private List<Color>         _colors = new List<Color>();
    private List<float>         _offsets = new List<float>();
    private List<int>           _triangles = new List<int>();

    public void Init()
    {
        //Size = 1;
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _transform = transform;
        CreateBody(Vertices);
        UpdateBody();

    }
    
	void FixedUpdate () 
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    AddNode(0.2f);
        //}
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    Grow(0.2f);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    Grow(-0.2f);
        //}
        //if ((Input.GetKeyDown(KeyCode.Space)))
        //{
        //    RemoveNode(_nodes[Random.Range(0, _nodes.Count)].body, 0.2f);
        //}
        UpdateVertices();
	}

    public void Grow(float by)
    {
        StartCoroutine(GrowRoutine(by));
    }

    public void RemoveNode(Rigidbody2D body, float shrinkBy)
    {
        var positions = new List<Vector3>();
        CachePositions(positions, _nodes);
        Disassemble();
        int index = _nodes.FindIndex(n => n.body.Equals(body));
        _nodes.RemoveAt(index);
        positions.RemoveAt(index);
        _radius -= shrinkBy * 0.5f;
        _center.GetComponent<CircleCollider2D>().radius = _radius;
        CreateVerticies(_nodes, _vertices);
        Assemble();
        UpdateBody();
        RevertPositions(positions, _nodes);
        Destroy(body.gameObject);
    }

    public void AddNode(float growBy)
    {
        List<Vector3> oldPos = new List<Vector3>();
        CachePositions(oldPos, _nodes);
        Disassemble();
        int index = Random.Range(1, _nodes.Count - 1);

        int prev = index == 0 ? _nodes.Count - 1 : index - 1;
        int next = (index + 1) % _nodes.Count;
        _nodes.Insert(index, new Node());
        _nodes[index].body = SoftBodyHelper.CreateRigidChild(new GameObject("New Node"), _transform, 
            Vector3.zero, _prototype, 0.1f);
        _nodes[index].collider = _nodes[index].body.GetComponent<CircleCollider2D>();
        _radius += growBy * 0.5f;
        _center.GetComponent<CircleCollider2D>().radius = _radius;
        CreateVerticies(_nodes, _vertices);
        oldPos.Insert(index, (_nodes[prev].body.position + _nodes[next].body.position) / 2);
        Assemble();
        UpdateBody();
        RevertPositions(oldPos, _nodes);        
    }

    private IEnumerator GrowRoutine(float by)
    {
        Disassemble();
        List<Vector3> velocities = new List<Vector3>();
        foreach (var node in _nodes)
        {

            velocities.Add(node.body.velocity);
        }
        _radius += by * 0.5f;
        _center.GetComponent<CircleCollider2D>().radius = _radius;
        CreateVerticies(_nodes, _vertices);
        Assemble();

        for(int i= 0; i < _nodes.Count; i++)
        {
            _nodes[i].body.velocity = velocities[i];
        }

        for (int i = 0; i < _nodes.Count; i++)
        {
            _nodes[i].body.AddForce(_nodes[i].body.transform.localPosition, ForceMode2D.Impulse);
        }
        yield return new WaitForSeconds(0.2f);
    }
    public int ChildIndex(Rigidbody2D child)
    {
        return _nodes.FindIndex(n => n.body.Equals(child));
    }
    public Rigidbody2D ChildAtIndex(int index)
    {
        return index > _nodes.Count - 1 ? _nodes[_nodes.Count - 1].body : _nodes[index].body;
    }

    #region creation
    private void CreateBody(int vertexCount)
    {
        _center = SoftBodyHelper.CreateRigidChild(gameObject, transform, Vector3.zero, _prototype, _radius, false);

        // add rigid bodies
        CreateNodes(_nodes, vertexCount);
        // set verticies
        CreateVerticies(_nodes, _vertices);
        //add spring joints
        Assemble();
    }

    private void CreateNodes(List<Node> nodes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Node node = new Node();
            node.body = SoftBodyHelper.CreateRigidChild(new GameObject("Node" + i.ToString()), _transform, Vector3.zero, _prototype, .1f);
            node.collider = node.body.GetComponent<CircleCollider2D>();
            nodes.Add(node);
        }
    }

    private void CreateVerticies(List<Node> nodes, List<Vector3> verticies)
    {
        float angle = (2 * Mathf.PI) / nodes.Count;
        float initialOffset = Random.Range(0, Mathf.PI);
        float randomOffsetSize = 0.25f;
        _vertices.Clear();
        _vertices.Add(Vector3.zero);
        for (int i = 0; i < nodes.Count; i++)
        {
            float randomOffset = Random.Range(-angle, angle) * randomOffsetSize;
            float angleOffset = initialOffset - i * angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius;
            nodes[i].body.position = _transform.TransformPoint(position);
            _vertices.Add(position * 2);
        }
    }
    #endregion
    #region utility
    private void CachePositions(List<Vector3> positoins, List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            positoins.Add(node.body.position);
        }

    }
    private void RevertPositions(List<Vector3> positoins, List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].body.position = positoins[i];
        }
    }

    private void Assemble()
    {
        //add spring joints
        for (int i = 0; i < _nodes.Count; i++)
        {
            Node node = _nodes[i];
            node.SetNode(Node.JointType.Left, _nodes[i == 0 ? _nodes.Count - 1 : i - 1], _frequency, _damping);
            node.SetNode(Node.JointType.Right, _nodes[(i + 1) % _nodes.Count], _frequency, _damping);
            node.SetBody(Node.JointType.Center, _center, _frequency, _damping);
            node.collider.radius = Mathf.Min(
                                                Vector3.Distance(node.body.position, _center.position),
                                                Vector3.Distance(node.body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].body.position),
                                                Vector3.Distance(node.body.position, _nodes[(i + 1) % _nodes.Count].body.position)
                                            );
        }
    }

    private void Disassemble()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (var type = Node.JointType.Center; type <= Node.JointType.Right; type++)
            {
                _nodes[i].ClearNode(type);
            }
        }
    }

    private IEnumerator UpdateDistances()
    {
        foreach (var node in _nodes)
        {
            node.JointCenter.distance = Vector3.Distance(node.body.position, _center.position);
            node.JointLeft.distance = Vector3.Distance(node.NodeLeft.body.position, node.body.position);
            node.JointRight.distance = Vector3.Distance(node.NodeRight.body.position, node.body.position);
            
        }
        yield return null;
    }
    #endregion
    #region drawing
    public void UpdateBody()
    {
        _triangles.Clear();
        _normals.Clear();
        _colors.Clear();
        int pivot = 0;
        for (int id = 1; id < _vertices.Count; id++)
        {
            int nextID = (id == (_vertices.Count - 1)) ? 1 : id + 1;
            _triangles.Add(pivot);
            _triangles.Add(id);
            _triangles.Add(nextID);
        }

        for (int id = 0; id < _vertices.Count; id++)
        {
            _normals.Add(Vector3.forward);
            _colors.Add(Color.white);
            
        }
        _colors[0] = new Color(1, 1, 1, 0);
        _mesh.Clear();
        UpdateVertices();
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetNormals(_normals);
        _mesh.SetColors(_colors);
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            var pos = _nodes[i].body.transform.localPosition;
            _vertices[i + 1] = pos + pos.normalized * (_nodes[i].collider.radius);
        }
        _mesh.SetVertices(_vertices);
        Vertices = _vertices.Count - 1;
    }
    #endregion
}
