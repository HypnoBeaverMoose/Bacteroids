using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class BaseSoftBody : MonoBehaviour,  ISoftBody
{

    protected Mesh              _mesh = null;
    protected PolygonCollider2D _collider;
    protected Rigidbody2D       _rigidbody;

    protected List<Vector3> _vertices = null;
    protected List<Vector3> _vertexVelocities;
    protected List<Vector3> _originalVerticies;
    protected List<Vector3> _normals = new List<Vector3>();
    protected List<Color>   _colors = new List<Color>();
    protected List<Vector2> _points = new List<Vector2>();
    protected List<int>     _triangles = new List<int>();

    public abstract Vector3 AverageVelocity { get; }

    public abstract void UpdateDeformation();

    public abstract Vector3 AddDeformingForce(Vector3 point, Vector3 force);

    public virtual void Init()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _vertices = new List<Vector3>();
        _originalVerticies = new List<Vector3>();
        _vertexVelocities = new List<Vector3>();
    }

    public virtual void UpdateBody()
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


}
