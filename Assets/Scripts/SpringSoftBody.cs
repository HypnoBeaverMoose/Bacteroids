using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpringSoftBody : BaseSoftBody
{
    private List<float> _initialDistances = null;
    private List<float> _distances = null;
    
    private void Start () 
    {
	    
	}

    public override void Init()
    {
        base.Init();
    //    _initialDistances = new List<Vector3>();
    //    _distances = new List<Vector3>();
    //    _vertices = new List<Vector3>(GenerateVertices(4));
    //    for (int i = 0; i < _vertices.Count; i++)
    //    {
    //        _vertexVelocities.Add(Vector3.zero);
    //        int nextindex = i < (_vertices.Count - 1) ? i + 1 : 1;
    //        if (i > 0)
    //        {
    //            _initialDistances.Add(Vector3.Distance(_vertices[i], _vertices[i + 1]));
    //        }
    //    }
    }


    private Vector3[] GenerateVertices(int count)
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
	// Update is called once per frame
	void Update () {
	
	}

    public override Vector3 AverageVelocity
    {
        get { throw new System.NotImplementedException(); }
    }

    public override void UpdateDeformation()
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 AddDeformingForce(Vector3 point, Vector3 force)
    {
        throw new System.NotImplementedException();
    }
}
