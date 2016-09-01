using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nucleus : MonoBehaviour
{
    [SerializeField]
    private int _pointsCount = 0;
    private LineRenderer _renderer;
    private List<Rigidbody2D> _anchors = new List<Rigidbody2D>();
    private List<Vector2> _offsets = new List<Vector2>();
    private bool _initialized = false;

    void Start()
    {
    }

    private void FixedUpdate()
    {
        if (_initialized)
        {
            for (int i = 0; i < _anchors.Count; i++)
            {
                if (_anchors[i] == null)
                {
                    Destroy(gameObject);
                    return;
                }
                _renderer.SetPosition(i, GetPosition(_anchors[i], _offsets[i]));
            }
        }
    }

    private Vector2 GetPosition(Rigidbody2D body, Vector2 offset)
    {
        return body.transform.TransformPoint(offset);
    }

    public void Generate(Node center, Node[] nodes, float radius)
    {
        StartCoroutine(DelayedGeneration(center, nodes, radius));
    }

    private IEnumerator DelayedGeneration(Node center, Node[] nodes, float radius)
    {
        yield return null;
        yield return null;

        _renderer = GetComponent<LineRenderer>();
        for (int i = 0; i < _pointsCount; i++)
        {
            int index = Random.Range(0, nodes.Length);
            Vector2 position = center.transform.position + (nodes[index].transform.position - center.transform.position).normalized * Random.Range(0, radius);

            Debug.Log(position);
            _anchors.Add(nodes[index].Body);
            _offsets.Add(nodes[index].transform.InverseTransformPoint(position));
            Debug.Log(nodes[index].transform.InverseTransformPoint(position));
        }

        _renderer.SetVertexCount(_anchors.Count);
        _initialized = true;
    }
}
