using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundSoftBody : MonoBehaviour 
{
    //[SerializeField]
    //private AnimationCurve _FrequencySize;
    //[SerializeField]
    //private AnimationCurve _DampingOverSize;
    //[SerializeField]
    //private AnimationCurve _DragOverSize;

 
    //[SerializeField]
    //private Rigidbody2D _prototype;
    //[SerializeField]
    //private float _damping;
    //[SerializeField] 
    //public float _frequency;
    
    public int Vertices;
    public float Size { get { return _radius * 2; } set { _radius = value / 2; } }
    
    
    private Mesh _mesh = null;

    private float               _radius = 0;
    private List<Node>          _nodes = new List<Node>();
    private Node                _center = null;
    private Transform           _transform = null;
    private List<Vector3>       _vertices = new List<Vector3>();
    private List<Vector3>       _normals = new List<Vector3>();
    private List<Color>         _colors = new List<Color>();
    private List<int>           _triangles = new List<int>();

    public void Init()
    {
        Size = 0.5f;
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _transform = transform;
        CreateBody(Vertices);
        UpdateBody();
    }
    
	void FixedUpdate () 
    {
        //for (int i = 0; i < _nodes.Count; i++)
        //{
        //    float freq = _FrequencySize.Evaluate(Size);
        //    float damping = _DampingOverSize.Evaluate(Size);
        //    float drag = _DragOverSize.Evaluate(Size);
        //    _nodes[i]._rigidbody.drag = drag;
        //    _nodes[i].JointCenter.frequency = freq;
        //    _nodes[i].JointLeft.frequency = freq;
        //    _nodes[i].JointRight.frequency = freq;



        //    _nodes[i].JointCenter.dampingRatio = damping;
        //    _nodes[i].JointLeft.dampingRatio = damping;
        //    _nodes[i].JointRight.dampingRatio = damping;
        //}
        //_center.drag = _DragOverSize.Evaluate(Size);
        UpdateVertices();
	}

    public void Grow(float by)
    {
        //StartCoroutine(GrowRoutine(by));
    }

    public void RemoveNode(Rigidbody2D body, float shrinkBy)
    {
        var positions = new List<Vector3>();
        var velocities = new List<Vector3>();
        CachePositions(positions, _nodes);
        CacheVelocities(velocities, _nodes);
        Disassemble();
        int index = _nodes.FindIndex(n => n.Body.Equals(body));
        _nodes.RemoveAt(index);
        positions.RemoveAt(index);
        velocities.RemoveAt(index);
        _radius -= shrinkBy * 0.5f;
        _center.GetComponent<CircleCollider2D>().radius = _radius;
        CreateVerticies(_nodes, _vertices);
        Assemble();
        UpdateBody();
        RevertPositions(positions, _nodes);
        RestoreVelocities(velocities, _nodes);
        Destroy(body.gameObject);
    }

    public void AddNode(float growBy)
    {
        return;
        //List<Vector3> oldPos = new List<Vector3>();
        //List<Vector3> velocities = new List<Vector3>();

        //CachePositions(oldPos, _nodes);
        //CacheVelocities(velocities, _nodes);
        //Disassemble();
        //int index = Random.Range(1, _nodes.Count - 1);

        //int prev = index == 0 ? _nodes.Count - 1 : index - 1;
        //int next = (index + 1) % _nodes.Count;
        //_nodes.Insert(index, new Node());
        //_nodes[index].Body = SoftBodyHelper.CreateRigidChild(new GameObject("New Node"), _transform, 
        //    Vector3.zero, _prototype, 0.1f);
        //_nodes[index].Collider = _nodes[index].B.GetComponent<CircleCollider2D>();
        //_radius += growBy * 0.5f;
        //_center.GetComponent<CircleCollider2D>().radius = _radius;
        //CreateVerticies(_nodes, _vertices);
        //oldPos.Insert(index, (_nodes[prev]._rigidbody.position + _nodes[next]._rigidbody.position) / 2);
        //velocities.Insert(index, Vector3.zero);
        //Assemble();
        //UpdateBody();
        //RevertPositions(oldPos, _nodes);
        //RestoreVelocities(velocities, _nodes);
    }

    private IEnumerator GrowRoutine(float by)
    {
        //Disassemble();
        //List<Vector3> velocities = new List<Vector3>();
        //CacheVelocities(velocities, _nodes);
        //_radius += by * 0.5f;
        //_center.GetComponent<CircleCollider2D>().radius = _radius;
        //CreateVerticies(_nodes, _vertices);
        //Assemble();

        //RestoreVelocities(velocities, _nodes);
        //for (int i = 0; i < _nodes.Count; i++)
        //{
        //    _nodes[i]._rigidbody.AddForce(_nodes[i]._rigidbody.transform.localPosition, ForceMode2D.Impulse);
        //}
        yield return new WaitForSeconds(0.2f);
    }
    public int ChildIndex(Rigidbody2D child)
    {
        return _nodes.FindIndex(n => n.Body.Equals(child));
    }

    public Rigidbody2D ChildAtIndex(int index)
    {
        ///Debug.Log(_nodes.Count + "    " + index);        
        return index > (_nodes.Count - 1) ? _nodes[_nodes.Count - 1].Body : _nodes[index].Body;
    }

    #region creation
    private void CreateBody(int vertexCount)
    {
        // add rigid bodies
        CreateNodes(_nodes, vertexCount);
        // set verticies
        CreateVerticies(_nodes, _vertices);
        //add spring joints
        Assemble();
    }

    private void CreateNodes(List<Node> nodes, int count)
    {
        //_center = SoftBodyHelper.CreateNode(gameObject, transform, Vector3.zero, _prototype, _radius, false);
        //for (int i = 0; i < count; i++)
        //{
        //    nodes.Add(SoftBodyHelper.CreateNode(new GameObject("Node" + i.ToString()), _transform, Vector3.zero, _prototype, _radius));
        //}
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
            nodes[i].Body.position = _transform.TransformPoint(position);
            _vertices.Add(position * 2);
        }
    }
    #endregion
    #region utility
    private void CachePositions(List<Vector3> positoins, List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            positoins.Add(node.Body.position);
        }

    }
    private void RevertPositions(List<Vector3> positoins, List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Body.position = positoins[i];
        }
    }

    private void Assemble()
    {
        ////add spring joints
        //for (int i = 0; i < _nodes.Count; i++)
        //{
        //    Node node = _nodes[i];
        //    node.Connect(Node.JointType.Left, _nodes[i == 0 ? _nodes.Count - 1 : i - 1], _frequency, _damping);
        //    node.Connect(Node.JointType.Right, _nodes[(i + 1) % _nodes.Count], _frequency, _damping);
        //    node.Connect(Node.JointType.Center, _center, _frequency, _damping);
        //    node.Collider.radius = Mathf.Min(
        //                                        Vector3.Distance(node.Body.position, _transform.position),
        //                                        Vector3.Distance(node.Body.position, _nodes[i == 0 ? _nodes.Count - 1 : i - 1].Body.position),
        //                                        Vector3.Distance(node.Body.position, _nodes[(i + 1) % _nodes.Count].Body.position)
        //                                    );
        //}
    }

    private void Disassemble()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            for (var type = Node.JointType.Center; type <= Node.JointType.Right; type++)
            {
                //_nodes[i].ClearNode(type);
            }
        }
    }

    private IEnumerator UpdateDistances()
    {
//        foreach (var node in _nodes)
//        {
//            //node.JointCenter.distance = Vector3.Distance(node._rigidbody.position, _center.position);
//            //node.JointLeft.distance = Vector3.Distance(node.NodeLeft._rigidbody.position, node._rigidbody.position);
//            //node.JointRight.distance = Vector3.Distance(node.NodeRight._rigidbody.position, node._rigidbody.position);            
//        }
        yield return null;
    }
    public void CacheVelocities(List<Vector3> velocities, List<Node> nodes)
    {
        foreach (var node in _nodes)
        {
            velocities.Add(node.Body.velocity);
        }
    }

    public void RestoreVelocities(List<Vector3> velocities, List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Body.velocity = velocities[i];
        }

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
        var lineR = GetComponent<LineRenderer>();
        for (int i = 0; i < _nodes.Count; i++)
        {
            var pos = _nodes[i].Body.transform.localPosition;
            _vertices[i + 1] = pos + pos.normalized * (_nodes[i].Collider.radius);
        }

        lineR.SetVertexCount((_nodes.Count + 1) * 3);
        for (int i = 0; i < (_nodes.Count + 1); i++)
        {
            int index = i % _nodes.Count;
            var node = _nodes[index];
            var prev = _nodes[index == 0 ? _nodes.Count - 1 : index - 1];
            var next = _nodes[(index + 1) % _nodes.Count];
            var pos = node.Body.transform.position + (node.Body.transform.position - transform.position).normalized * node.Collider.radius;
            var prev_pos = prev.Body.transform.position + (prev.Body.transform.position - transform.position).normalized * prev.Collider.radius;
            var next_pos = next.Body.transform.position + (next.Body.transform.position - transform.position).normalized * next.Collider.radius;

            lineR.SetPosition(i * 3, pos + (prev_pos - pos).normalized * 0.01f);
            lineR.SetPosition(i * 3 + 1, pos);
            lineR.SetPosition(i * 3 + 2, pos + (next_pos - pos).normalized * 0.01f);
        }

        //lineR.SetVertexCount(_vertices.Count);
        //for (int i = 1; i < _vertices.Count; i++)
        //{
        //    lineR.SetPosition(i - 1, _transform.TransformPoint(_vertices[i]));
        //    //lineR.SetPosition(i * 2 + 1, _transform.TransformPoint(_vertices[i]) - (_transform.TransformPoint(_vertices[i]) - _transform.TransformPoint(_vertices[i + 1])) * 0.1f);
        //}
        //lineR.SetPosition(_vertices.Count - 1, _transform.TransformPoint(_vertices[1]));
        //lineR.SetPosition(0, _transform.TransformPoint(_vertices[0]) - (_transform.TransformPoint(_vertices[0]) - _transform.TransformPoint(_vertices[_vertices.Count - 1])) * 0.1f);
        //lineR.SetPosition(1, _transform.TransformPoint(_vertices[0]) - (_transform.TransformPoint(_vertices[0]) - _transform.TransformPoint(_vertices[1])) * 0.1f);

        //lineR.SetPosition(_vertices.Count - 2, _transform.TransformPoint(_vertices[_vertices.Count - 2]) - (_transform.TransformPoint(_vertices[_vertices.Count - 2]) - _transform.TransformPoint(_vertices[_vertices.Count - 3])) * 0.1f);
        //lineR.SetPosition(_vertices.Count - 1, _transform.TransformPoint(_vertices[_vertices.Count - 1]) - (_transform.TransformPoint(_vertices[_vertices.Count - 1]) - _transform.TransformPoint(_vertices[0])) * 0.1f);

        _mesh.SetVertices(_vertices);
        Vertices = _vertices.Count - 1;
    }
    #endregion
}

