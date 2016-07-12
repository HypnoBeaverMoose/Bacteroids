using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SimpleSoftBody : MonoBehaviour//, ISoftBody
{

    public float damping = 0.1f;
    public float springForce =1f;
    public float mass = 0.25f;
    private Mesh                _mesh = null;
    private PolygonCollider2D   _collider;
    private Rigidbody2D         _rigidbody;

    private Vector3[]       _startVertices;
    private List<Vector3>   _vertices = new List<Vector3>();
    private List<Vector3>   _vertexVelocities = new List<Vector3>();
    private List<Vector3>   _originalVerticies = new List<Vector3>();
    private List<Vector3>   _worldVertices = new List<Vector3>();
    private List<Vector3>   _normals = new List<Vector3>();
    private List<Vector2>   _points = new List<Vector2>();
    private List<Color>     _colors = new List<Color>();
    private List<int>       _triangles = new List<int>();  
    
    public void Init()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _startVertices = GenerateStartVertices(4);
        _vertices = new List<Vector3>(_startVertices);
        //_originalVerticies = new List<Vector3>(_startVertices);
        _vertexVelocities = new List<Vector3>();

        for (int id = 0; id < _vertices.Count; id++)
        {
            _originalVerticies.Add(transform.TransformPoint(_startVertices[id]));
            _worldVertices.Add(transform.TransformPoint(_startVertices[id]));
            _vertexVelocities.Add(Vector3.zero);
            if (id > 0)
            {
                _points.Add(_vertices[id]);
            }
        }
        _collider.points = _points.ToArray();
    }

    public void UpdateDeformation()
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            _originalVerticies[i] = transform.TransformPoint(_startVertices[i]);            
            //_worldVertices[i] = transform.TransformPoint(_vertices[i]);
            Vector3 velocity = _vertexVelocities[i];
            //Vector3 displacement = (_worldVertices[i] - _originalVerticies[i]) / Time.deltaTime;
            //velocity -= displacement * springForce * Time.fixedDeltaTime;
            //velocity *= 1f - damping * Time.fixedDeltaTime;            
            _vertexVelocities[i] = velocity + (_worldVertices[i] - _originalVerticies[i]) / Time.deltaTime;
            _worldVertices[i] += (_vertexVelocities[i] + (Vector3)_rigidbody.velocity) * Time.fixedDeltaTime;

            _vertices[i] = transform.InverseTransformPoint(_worldVertices[i]);
        }
    }

    private Vector3[] GenerateStartVertices(int count)
    {
        float inc = (2 * Mathf.PI) / count;
        float initialOffset = Random.Range(0, Mathf.PI);
        
        Vector3[] vertices = new Vector3[count + 1];
        vertices[0] = Vector3.zero;
        float sqrt2 = 1.0f / Mathf.Sqrt(2);
        for (int i = 0; i < count; i++)
        {
            float angle = initialOffset - i * inc + Random.Range(-inc, inc) / 2;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * sqrt2;
        }
        return vertices;
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
        Vector2[] pnts = new Vector2[_collider.points.Length];
        for (int id = 0; id < _vertices.Count; id++)
        {
            _normals.Add(Vector3.forward);
            if (id > 0)
            {
              pnts[id - 1] = _vertices[id];
            }
            _colors.Add(Color.white);

        }
        _colors[0] = new Color(1, 1, 1, 0);
        _mesh.Clear();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetNormals(_normals);
        _mesh.SetColors(_colors);
        //_collider.points = pnts;
    }

    public void AddDeformingForce(Vector3 point, Vector3 force)
    {
        //point = transform.InverseTransformPoint(point);
        //for (int i = 0; i < _vertices.Count; i++)
        //{
        //    Vector3 pointToVertex = _vertices[i] - point;
        //    float attenuatedForce = force.magnitude / (1f + pointToVertex.sqrMagnitude);
        //    float velocity = attenuatedForce * Time.fixedDeltaTime / mass;
        //    _vertexVelocities[i] += force.normalized * velocity;
        //}
    }
}
