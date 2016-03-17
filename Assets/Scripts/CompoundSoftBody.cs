using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompoundSoftBody : MonoBehaviour 
{    
    public Rigidbody2D BodyPrototype;
    public float Damping;
    public float Frequency;
    public int Vertices;
    public float Size = 1.0f;
    private Mesh _mesh = null;

    private List<Rigidbody2D>   _bodies = new List<Rigidbody2D>();
    private List<Vector3>       _vertices = new List<Vector3>();
    private List<Vector3>       _normals = new List<Vector3>();
    private List<Color>         _colors = new List<Color>();
    private List<float>         _offsets = new List<float>();
    private List<int>           _triangles = new List<int>();
    private List<SpringJoint2D> _joints = new List<SpringJoint2D>();

    public void Init()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();
        _vertices = new List<Vector3>(GenerateVertices(Vertices));
        UpdateBody();

    }
    
	void FixedUpdate () 
    {        
        if (Random.value < 0.005f && _bodies[0].velocity.magnitude < 0.1f)
        {
            var body = _bodies[Random.Range(1, _bodies.Count)];
            body.AddForce(body.transform.localPosition.normalized * 10, ForceMode2D.Impulse);
        }

        for (int i = 0; i < _vertices.Count; i++)
        {
            var pos = transform.InverseTransformPoint(_bodies[i].position);
            _vertices[i] = pos + pos.normalized * (_offsets[i] + _bodies[i].GetComponent<CircleCollider2D>().radius);
        }

        UpdateBody();
	}

    private Vector3[] GenerateVertices(int count)
    {
        float inc = (2 * Mathf.PI) / count;
        float initialOffset =  Random.Range(0, Mathf.PI);

        Vector3[] vertices = new Vector3[count + 1];
        vertices[0] = Vector3.zero;

        float bigR = Size;
        
        for (int i = 0; i < count; i++)
        {
            float randomOffset = Random.Range(-inc, inc) / 4;
            float angle = initialOffset - i * inc + randomOffset;
            Vector3 vec = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle),0);
            vertices[i + 1] = vec * bigR;
        }
        var center = CreateRigidChild(gameObject, vertices[0], Size / 2, false);
        
        _bodies.Add(center);
        _offsets.Add(0);

        for (int i = 1; i < vertices.Length; i++)
        {
            int next = i == vertices.Length - 1 ? 1 : i + 1;
            var pos = vertices[i] * 0.5f;
            var body = CreateRigidChild(new GameObject("Element"), pos, Vector3.Cross((vertices[next] - vertices[i]).normalized, pos - vertices[i]).magnitude);
            _joints.Add(CreateSpringJoint(body.gameObject, center));
            _bodies.Add(body);
            _offsets.Add((vertices[i] - body.transform.localPosition).magnitude - body.GetComponent<CircleCollider2D>().radius);
        }
        for (int i = 1; i < _bodies.Count; i++)
        {
            int next = i == _bodies.Count - 1 ? 1 : i + 1;
            int prev = i == 1 ? _bodies.Count - 1 : i - 1;
           _joints.Add(CreateSpringJoint(_bodies[i].gameObject, _bodies[next]));
           _joints.Add(CreateSpringJoint(_bodies[i].gameObject, _bodies[prev]));
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

        for (int id = 0; id < _vertices.Count; id++)
        {
            _normals.Add(Vector3.forward);
            _colors.Add(Color.white);
            
        }
        _colors[0] = new Color(1, 1, 1, 0);
        _mesh.Clear();
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangles, 0);
        _mesh.SetNormals(_normals);
        _mesh.SetColors(_colors);
    }

    private SpringJoint2D CreateSpringJoint(GameObject obj, Rigidbody2D anchor)
    {
        var spring = obj.AddComponent<SpringJoint2D>();
        spring.connectedBody = anchor;
        spring.distance = (obj.transform.position - anchor.transform.position).magnitude;
        spring.frequency = Frequency;
        spring.dampingRatio = Damping;
        spring.autoConfigureConnectedAnchor = true;
        spring.autoConfigureDistance = true;
        return spring;
    }

    private Rigidbody2D CreateRigidChild(GameObject go, Vector3 position, float radius, bool setPos = true)
    {
        go.layer = LayerMask.NameToLayer("Bacteria");
        go.AddComponent<CircleCollider2D>().radius = radius;
        if (setPos)
        {
            var tr = go.transform;
            tr.SetParent(transform, true);
            tr.localPosition = position;
            tr.rotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }
        var body = go.AddComponent<Rigidbody2D>();
        body.mass = BodyPrototype.mass;
        body.drag = BodyPrototype.drag;
        body.angularDrag = BodyPrototype.angularDrag;
        body.gravityScale = BodyPrototype.gravityScale;
        go.AddComponent<CollisionHandler>();
        return body;
    }

}
