using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public class MeshBacteria : MonoBehaviour 
{
    public float GrowSpeed = 1.0f;
    public int MaxVertices = 8;
    public float Energy { get; set; }
    
    [SerializeField]
    private GameObject _energy;
    [SerializeField]
    private float _startEnergy;

    private Mesh _mesh = null;
    private Rigidbody2D _rigidbody = null;
    private PolygonCollider2D _collider;


    private List<Vector3> _vertices = null;
    private List<Vector3> _vertexVelocities;
    private List<Vector3> _originalVerticies;
    private List<float> _vertexCoDistance = null;
    private List<Vector3> _normals = new List<Vector3>();
    private List<Vector2> _points = new List<Vector2>();
    private List<int> _triangles = new List<int>();
    private static Vector3[] _startVertices = new Vector3[] {   Vector3.zero,
                                                                new Vector3(-.5f, .5f, 0),
                                                                new Vector3(.5f, .5f, 0),
                                                                new Vector3(.5f, -.5f, 0),
                                                                new Vector3(-.5f, -.5f, 0) };
    private void Start()
    {
        Init();
        _collider = GetComponent<PolygonCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        Energy = _startEnergy;
        UpdateMesh();
    }
    
    private void Init()
    {
        _vertices = new List<Vector3>(_startVertices);
        _originalVerticies = new List<Vector3>(_startVertices);
        _vertexVelocities = new List<Vector3>();
        foreach (var vertex in _vertices)
        {
            _vertexVelocities.Add(Vector3.zero);
        }
    }

    private void UpdateMesh()
    {
        _triangles.Clear();
        _normals.Clear();
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
            if (id > 0)
            {
                _points.Add(_vertices[id]);
            }
        }
        _mesh.Clear();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetNormals(_normals);
        _mesh.SetUVs(0, _vertices);        
        //_mesh.UploadMeshData(false);
        //_collider.points = _points.ToArray();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 force = (collision.relativeVelocity * collision.rigidbody.mass) / Time.fixedDeltaTime;
        AddDeformingForce(collision.contacts[0].point, force);
        if (collision.gameObject.CompareTag("Projectile"))
        {            
            _rigidbody.AddForceAtPosition(-force , collision.contacts[0].point);            
            var go = Instantiate<GameObject>(_energy);
            float damage = Random.Range(5, 20);
            this.Energy -= damage;
            go.GetComponent<Energy>().Amount = damage;
            go.transform.position = collision.contacts[0].point;
            go.GetComponent<Rigidbody2D>().AddForceAtPosition(-force.magnitude * collision.contacts[0].normal, collision.contacts[0].point);
        }
        


    }

    public void AddDeformingForce(Vector3 point, Vector3 force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < _vertices.Count; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    void AddForceToVertex(int i, Vector3 point, Vector3 force)
    {
        Vector3 pointToVertex = _vertices[i] - point;
        float attenuatedForce = force.magnitude / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.fixedDeltaTime;
        _vertexVelocities[i] += force.normalized * velocity;
       // _rigidbody.velocity -= (Vector2)((force.normalized * velocity) /(float)_vertexVelocities.Count);
    }
    void FixedUpdate()
    {
        float damping = 0.5f;
        float springForce = 20;
        Vector3 vel = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector3 velocity = _vertexVelocities[i];
            vel += velocity;
            Vector3 displacement = _vertices[i] - _originalVerticies[i];
            velocity -= displacement * springForce * Time.fixedDeltaTime;
            velocity *= 1f - damping * Time.fixedDeltaTime;
            _vertexVelocities[i] = velocity;
            _vertices[i] += velocity * Time.fixedDeltaTime;            
        }

        //_rigidbody.velocity = -(vel / _vertexVelocities.Count) * Time.deltaTime;
        UpdateMesh();
    }
    void Update()
    {

        if (Energy <= 0)
        {
            Destroy(gameObject);
        }
    }

}