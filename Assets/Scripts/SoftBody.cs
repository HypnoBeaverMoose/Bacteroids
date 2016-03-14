using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]

public class SoftBody : BaseSoftBody
{
    public int VertexCount = 4;
    public static float DAMPING = 0.5f;
    public static float SPRINGFORCE = 8;
    private Vector3 _averageVelocity = Vector3.zero;
    private List<Vector3> _worldVerticies;
    private List<Vector3> _worldOriginalVertices;
    private List<float> _masses;
    private Vector3[] _startVerticies;
    public override Vector3 AverageVelocity
    {
        get { return _averageVelocity; }
    }
    private int lastVertexCount;
    public override void Init()
    {
        base.Init();
        lastVertexCount = VertexCount;
        GenerateMesh(VertexCount);
    }

    public void GenerateMesh(int vertexCount)
    {
        _startVerticies = GenerateStartVertices(vertexCount);
        _vertices = new List<Vector3>(_startVerticies);
        _vertexVelocities.Clear();
        _points.Clear();
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

    void Update()
    {
        if (lastVertexCount != VertexCount)
        {
            lastVertexCount = VertexCount;
            GenerateMesh(VertexCount);
        }

    }
    public override void UpdateDeformation()
    {
        
        //_averageVelocity = Vector3.zero;
        //for (int i = 0; i < _vertices.Count; i++)
        //{
        //    _worldOriginalVertices[i] = transform.TransformPoint(_startVerticies[i]);
        //    Vector3 velocity = _vertexVelocities[i];

        //    Vector3 displacement = _vertices[i] - _originalVerticies[i];
        //    velocity -= displacement * SPRINGFORCE * Time.fixedDeltaTime;
        //    velocity *= 1f - DAMPING * Time.fixedDeltaTime;
            
        //    _vertexVelocities[i] = velocity;
        //    _vertices[i] += (velocity) * Time.fixedDeltaTime;
        //}
    }
    
    private Vector3[] GenerateStartVertices(int count)
    {
        float inc = (2 * Mathf.PI) / count;
        float initialOffset = Random.Range(0, 2 * Mathf.PI);

        Vector3[] vertices = new Vector3[count + 1];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            float angle = initialOffset + i * inc;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        return vertices;
    }
    public override Vector3 AddDeformingForce(Vector3 point, Vector3 force)
    {
        //Vector3 average = Vector3.zero;
        //point = transform.InverseTransformPoint(point);
        
        //for (int i = 0; i < _vertices.Count; i++)
        //{

        //    Vector3 pointToVertex = _vertices[i] - point;
        //    float attenuatedForce = force.magnitude / (1f + pointToVertex.sqrMagnitude);
        //    float velocity = attenuatedForce * Time.fixedDeltaTime / _masses[i];
        //    if (i == 0)
        //    {
        //        average = force.normalized * velocity;
        //    }
        //    else
        //    {
        //        _vertexVelocities[i] += force.normalized * velocity - average;
        //    }
        //}

        return Vector3.zero;
    }
}
