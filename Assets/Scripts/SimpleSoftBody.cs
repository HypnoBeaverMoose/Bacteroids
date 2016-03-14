using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SimpleSoftBody : BaseSoftBody
{
    private static Vector3[] _startVertices = new Vector3[] {   Vector3.zero,
                                                                new Vector3(-.5f, .5f, 0),
                                                                new Vector3(.5f, .5f, 0),
                                                                new Vector3(.5f, -.5f, 0),
                                                                new Vector3(-.5f, -.5f, 0) };


    public static float DAMPING = 0.5f;
    public static float SPRINGFORCE = 8;
    private Vector3 _averageVelocity = Vector3.zero;

    public override Vector3 AverageVelocity
    {
        get { return _averageVelocity; }
    }

    public override void Init()
    {
        base.Init();
        _vertices = new List<Vector3>(_startVertices);
        _originalVerticies = new List<Vector3>(_startVertices);
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

    public override void UpdateDeformation()
    {
        _averageVelocity = Vector3.zero;
        for (int i = 0; i < _vertices.Count; i++)
        {
            Vector3 velocity = _vertexVelocities[i];

            Vector3 displacement = _vertices[i] - _originalVerticies[i];
            velocity -= displacement * SPRINGFORCE * Time.fixedDeltaTime;
            velocity *= 1f - DAMPING * Time.fixedDeltaTime;
            
            _vertexVelocities[i] = velocity;
            _vertices[i] += (velocity) * Time.fixedDeltaTime;
        }
    }

    public override Vector3 AddDeformingForce(Vector3 point, Vector3 force)
    {
        Vector3 average = Vector3.zero;
        point = transform.InverseTransformPoint(point);
        
        for (int i = 0; i < _vertices.Count; i++)
        {

            Vector3 pointToVertex = _vertices[i] - point;
            float attenuatedForce = force.magnitude / (1f + pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.fixedDeltaTime * 2;
            if (i == 0)
            {
                average = force.normalized * velocity;
            }
            else
            {
                _vertexVelocities[i] += force.normalized * velocity - average;
            }
        }
        return average;
    }
}
