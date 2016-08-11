using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class BacteriaDrawer : MonoBehaviour 
{
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            if (_renderer != null)
            {
                _renderer.SetColors(_color, _color);
            }
        }
    }
    public float Width
    {
        get
        {
            return _width;
        }
        set 
        {
            _width = value;
            if (_renderer != null)
            {
                _renderer.SetWidth(_width, _width);
            }
        }
    }
        
    private LineRenderer _renderer;
    private List<Node> _nodes;
    [SerializeField]
    private float _attachableFrquency = 20;

    [SerializeField]
    private Color _color = new Color();
    [SerializeField]
    private float _width = 0.1f;
    [SerializeField]
    private GameObject _energyPrefab;

    private void Awake()
    {
        _renderer = GetComponent<LineRenderer>();
        _renderer.SetColors(Color, Color);       
        _renderer.SetWidth(_width, _width);
        _renderer.useWorldSpace = true;
        if (_nodes != null)
        {
            _renderer.SetVertexCount((_nodes.Count + 1) * 3);
            DrawOutline();
        }
    }

    public void Init(List<Node> nodes)
    {
        _nodes = new List<Node>(nodes);
        if (_renderer != null)
        {
            _renderer.SetVertexCount((_nodes.Count + 1) * 3);
            DrawOutline();
            foreach (var node in _nodes)
            {
                if (node.Collider.radius > 0.1f)
                {
                    var energy = ((GameObject)Instantiate(_energyPrefab, node.Body.position + (Random.insideUnitCircle * node.Collider.radius * 0.5f), Quaternion.identity)).GetComponent<Energy>();
                    SoftBodyHelper.CreateSpringJoint(energy.gameObject,  node.Body, _attachableFrquency, 1.0f);
                    energy.transform.localScale = Vector3.one * node.Collider.radius * Random.Range(0.3f, 0.6f);
                    energy.transform.parent = node.gameObject.transform;
                }

            }
        }

    }

    private void DrawOutline()
    {
        
        for (int i = 0; i < (_nodes.Count + 1); i++)
        {
            int index = i % _nodes.Count;
            var node = _nodes[index];
            var prev = _nodes[index == 0 ? _nodes.Count - 1 : index - 1];
            var next = _nodes[(index + 1) % _nodes.Count];
            var pos = node.Body.transform.position + (node.Body.transform.position - transform.position).normalized * node.Collider.radius;
            var prev_pos = prev.Body.transform.position + (prev.Body.transform.position - transform.position).normalized * prev.Collider.radius;
            var next_pos = next.Body.transform.position + (next.Body.transform.position - transform.position).normalized * next.Collider.radius;

            _renderer.SetPosition(i * 3, pos + (prev_pos - pos).normalized * 0.01f);
            _renderer.SetPosition(i * 3 + 1, node.Body.transform.position + (node.Body.transform.position - transform.position).normalized * node.Collider.radius * 2);
            _renderer.SetPosition(i * 3 + 2, pos + (next_pos - pos).normalized * 0.01f);
        }
    }

	private void Update () 
    {
        if (_nodes != null && _renderer != null)
        {
            DrawOutline();
        }
	}
}