using UnityEngine;
using System.Collections;

public class ShellDrawer : MonoBehaviour {

    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            var realColor = new Color(_color.r, _color.g, _color.b, alpha);
            _renderer.SetColors(realColor, realColor);
        }
    }
    public float Width
    {
        get { return _width; }
        set
        {
            _width = value;
            _renderer.SetWidth(_width, _width);
        }
    }

    [SerializeField]
    private float alpha;
    [SerializeField]
    private Color _color = new Color();
    [SerializeField]
    private float _width;
    [SerializeField]
    private float _param;
    [SerializeField]
    private float _offset;
    [SerializeField]
    private float _bevelSize;
    [SerializeField]
    private float _hornSize;
    [SerializeField]
    private LineRenderer _renderer;
    [SerializeField]
    private float _overallOffset;

    private Vector2 _randomOffset;

    private Vector2 _centerOfMass;
    private Node[] _nodes;
    private Vector3[] _positions;
    private void Awake()
    {
        _renderer.enabled = false;
        _randomOffset = Random.insideUnitCircle * _overallOffset;
    }

    public void Init(Node[] nodes)
    {
        _nodes = nodes;
        _renderer.SetVertexCount((_nodes.Length + 1) * 3);
        _positions = new Vector3[(_nodes.Length + 1) * 3];
        _renderer.SetWidth(_width, _width);
        var realColor = new Color(_color.r, _color.g, _color.b, alpha);
        _renderer.SetColors(realColor, realColor);
        _renderer.enabled = true;
        UpdatePositions();
        _renderer.SetPositions(_positions);
    }

    public void Clear()
    {
        _nodes = null;
        _renderer.SetVertexCount(0);
        _renderer.enabled = false;
    }

    private void UpdatePositions()
    {
        _centerOfMass = Vector2.zero;
        foreach (var node in _nodes)
        {
            _centerOfMass += node.Body.position;
        }
        _centerOfMass /= _nodes.Length;
        _renderer.SetWidth(_width, _width);
        for (int i = 0; i < (_nodes.Length + 1); i++)
        {
            int index = i % _nodes.Length;
            var node = _nodes[index];
            var prev = _nodes[index == 0 ? _nodes.Length - 1 : index - 1];
            var next = _nodes[(index + 1) % _nodes.Length];
            var pos = node.Body.position + (node.Body.position - _centerOfMass).normalized * node.Collider.radius * _param;
            var prev_pos = prev.Body.position + (prev.Body.position - _centerOfMass).normalized * prev.Collider.radius * _param;
            var next_pos = next.Body.position + (next.Body.position - _centerOfMass).normalized * next.Collider.radius * _param;

            var normal = (node.Body.position - _centerOfMass).normalized;
            _positions[i * 3] = pos + (prev_pos - pos).normalized * _bevelSize + normal * _offset + _randomOffset;
            _positions[i * 3 + 1] = node.Body.position + normal * node.Collider.radius * _hornSize * _param + normal * _offset + _randomOffset;
            _positions[i * 3 + 2] = pos + (next_pos - pos).normalized * _bevelSize + normal * _offset + _randomOffset;
        }
    }

	void Update ()
    {
        if (_nodes == null)
        {
            return;            
        }

        UpdatePositions();
        _renderer.SetPositions(_positions);
    }
}
