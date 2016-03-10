using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SimpleSoftBody : MonoBehaviour, ISoftBody
{
    private Mesh _mesh = null;
    private PolygonCollider2D _collider;
    private Rigidbody2D _rigidbody;
    private Vector3 _averageVelocity = Vector3.zero;
    private Vector3 _test = Vector3.zero;
    private float damping = 0.5f;
    private float springForce = 8;

    private List<Vector3> _vertices = null;
    private List<Vector3> _vertexVelocities;
    private List<Vector3> _originalVerticies;
    private List<float> _vertexCoDistance = null;
    private List<Vector3> _normals = new List<Vector3>();
    private List<Color> _colors = new List<Color>();
    private List<Vector2> _points = new List<Vector2>();
    private List<int> _triangles = new List<int>();
    private static Vector3[] _startVertices = new Vector3[] {   Vector3.zero,
                                                                new Vector3(-.5f, .5f, 0),
                                                                new Vector3(.5f, .5f, 0),
                                                                new Vector3(.5f, -.5f, 0),
                                                                new Vector3(-.5f, -.5f, 0) };

    public Vector3 AverageVelocity
    {
        get { return _averageVelocity; }
    }

    public void Init()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _vertices = new List<Vector3>(_startVertices);
        _originalVerticies = new List<Vector3>(_startVertices);
        _vertexVelocities = new List<Vector3>();
        for (int id = 0; id < _vertices.Count; id++)
        {
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
        _averageVelocity = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector3 velocity = _vertexVelocities[i];

            Vector3 displacement = _vertices[i] - _originalVerticies[i];
            velocity -= displacement * springForce * Time.fixedDeltaTime;
            velocity *= 1f - damping * Time.fixedDeltaTime;
            
            _vertexVelocities[i] = velocity;
            _vertices[i] += (velocity) * Time.fixedDeltaTime;
        }
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
        _collider.points = pnts;
    }

    public Vector3 AddDeformingForce(Vector3 point, Vector3 force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector3 pointToVertex = _vertices[i] - point;
            float attenuatedForce = force.magnitude / (1f + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.fixedDeltaTime;
            _vertexVelocities[i] += force.normalized * velocity;
        }
        return _vertexVelocities[0];
    }
}
