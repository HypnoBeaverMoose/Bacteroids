using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundSoftBody : MonoBehaviour 
{
    private class Node
    {
        public Rigidbody2D body;
        public CircleCollider2D collider;
        public Node leftNode;
        public Node rightNode;
        public SpringJoint2D leftJoint;
        public SpringJoint2D rightJoint;
        public SpringJoint2D centerJoint;
    }

    [SerializeField]
    private Rigidbody2D _prototype;
    [SerializeField]
    private float _damping;
    [SerializeField]
    public float _frequency;
    [SerializeField]
    private int _startVertices;
    [SerializeField]
    private float _startSize = 1.0f;
    
    
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
        _radius = _startSize / 2;
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _transform = transform;
        CreateBody(_startVertices);        
        UpdateBody();

    }
    
	void FixedUpdate () 
    {
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddNode();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Grow(0.2f);
        }

        UpdateVertices();
	}
    private void CreateBody(int vertexCount)
    {
        float angle = (2 * Mathf.PI) / vertexCount;
        float initialOffset = Random.Range(0, Mathf.PI);
        float randomOffsetSize = 0.25f;
        _center = SoftBodyHelper.CreateRigidChild(gameObject, transform, Vector3.zero, _prototype, _radius, false);
        _vertices.Add(Vector3.zero);
        // add rigid bodies
        for (int i = 0; i < vertexCount; i++)
        {
            Node node = new Node();
            float randomOffset = Random.Range(-angle, angle) * randomOffsetSize;
            float angleOffset = initialOffset - i * angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius;
            node.body = SoftBodyHelper.CreateRigidChild(new GameObject("Node"), _transform, position, _prototype, _radius);
            node.collider = node.body.GetComponent<CircleCollider2D>();
            _vertices.Add(position * 2); 
            _nodes.Add(node);
        }
        
        //add spring joints
        for (int i = 0; i < _nodes.Count; i++)
        {
            Node node = _nodes[i];
            Node right = _nodes[(i + 1) % _nodes.Count];
            Node left = _nodes[i == 0 ? _nodes.Count - 1 : i - 1];            
            node.centerJoint = SoftBodyHelper.CreateSpringJoint(node.body.gameObject, _center, _frequency, _damping);
            node.leftNode = left;
            node.leftJoint = SoftBodyHelper.CreateSpringJoint(node.body.gameObject, left.body, _frequency, _damping);
            node.rightNode = right;
            node.rightJoint = SoftBodyHelper.CreateSpringJoint(node.body.gameObject, right.body, _frequency, _damping);
        }
    }

    public void Grow(float by)
    {
        StartCoroutine(GrowRoutine(by));
    }

    public void AddNode()
    {
        StartCoroutine(AddNodeRoutine());
    }
    
    private IEnumerator AddNodeRoutine()
    {

        int index = 1;// Random.Range(1, _nodes.Count - 1);
        Node prev = _nodes[index - 1];
        Node next = _nodes[index];
        var pos = ((next.body.position - prev.body.position) * 0.5f);
        Node newNode = new Node();
        newNode.body = SoftBodyHelper.CreateRigidChild(new GameObject("NewNode"), _transform, pos, _prototype, _radius);
        newNode.centerJoint = SoftBodyHelper.CreateSpringJoint(newNode.body.gameObject, _center, _frequency, _damping);
        newNode.leftJoint = SoftBodyHelper.CreateSpringJoint(newNode.body.gameObject, prev.body, _frequency, _damping);
        newNode.rightJoint = SoftBodyHelper.CreateSpringJoint(newNode.body.gameObject, next.body, _frequency, _damping);
        newNode.collider = newNode.body.GetComponent<CircleCollider2D>();
        newNode.leftNode = prev;
        newNode.rightNode = next;

        prev.rightNode = newNode;
        prev.rightJoint.connectedBody = newNode.body;
        next.leftNode = newNode;
        next.leftJoint.connectedBody = newNode.body;

        newNode.body.transform.SetSiblingIndex(index);
        _nodes.Insert(index, newNode);
        
        float angle = (2 * Mathf.PI) / _nodes.Count;
        float initialOffset = Random.Range(0, Mathf.PI);
        float randomOffsetSize = 0.5f;
        _vertices.Clear();
        _vertices.Add(Vector3.zero);
        for (int i = 0; i < _nodes.Count; i++)
        {
            float randomOffset = Random.Range(-angle, angle) * randomOffsetSize;
            float angleOffset = initialOffset - i * angle + randomOffset;
            Vector3 position = new Vector3(Mathf.Cos(angleOffset), Mathf.Sin(angleOffset), 0) * _radius;
            _nodes[i].body.position = transform.TransformPoint(position);
            _vertices.Add(position);
        }
        UpdateBody();
        yield return new WaitForSeconds(1.0f);
        UpdateDistances();        
    }

    private IEnumerator GrowRoutine(float by)
    {
        _radius += by * 0.5f;
        _center.GetComponent<CircleCollider2D>().radius = _radius;
        foreach (var node in _nodes)
        {
            node.centerJoint.distance = _radius;
            node.collider.radius = _radius;
            node.body.AddForce((node.body.position - _center.position) * 10, ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.0f);        
        //not too sure about this
        UpdateDistances();
    }
  
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
    }
    private void UpdateDistances()
    {
        foreach (var node in _nodes)
        {
            node.centerJoint.connectedBody = _center;
            node.leftJoint.connectedBody = node.leftNode.body;
            node.rightJoint.connectedBody = node.rightNode.body;
        }
    }
}
