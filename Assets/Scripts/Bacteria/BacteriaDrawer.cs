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
        
    private Bacteria _bacteria;
    private LineRenderer _renderer;

    [SerializeField]
    private Color _color = new Color();
    [SerializeField]
    private float _width = 0.1f;
    [SerializeField]
    private GameObject _attachablePrefab;
        
    private void Awake()
    {
        _renderer = GetComponent<LineRenderer>();
        _renderer.SetColors(Color, Color);       
        _renderer.SetWidth(_width, _width);
        _renderer.useWorldSpace = true;
        if (_bacteria != null)
        {
            _renderer.SetVertexCount((_bacteria.Vertices + 1) * 3);
            DrawOutline();
        }
    }

    private void InitAttachables()
    {        
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            var node = _bacteria[i];
            if (node.Collider.radius > 0.17f)
            {
                var attachable = ((GameObject)Instantiate(_attachablePrefab, node.Body.position, Quaternion.identity)).GetComponent<Attachable>();
                attachable.AttachTo(node.Body, Vector2.zero);
                attachable.transform.parent = transform;
            }
        }
    }

    public void Init(Bacteria bacteria)
    {
        _bacteria = bacteria;
        InitAttachables();
        if (_renderer != null)
        {
            _renderer.SetVertexCount((_bacteria.Vertices + 1) * 3);
            DrawOutline();
        }
    }

    private void DrawOutline()
    {
        
        for (int i = 0; i < (_bacteria.Vertices + 1); i++)
        {
            int index = i % _bacteria.Vertices;
            var node = _bacteria[index];
            var prev = _bacteria[index == 0 ? _bacteria.Vertices - 1 : index - 1];
            var next = _bacteria[(index + 1) % _bacteria.Vertices];
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
        if (_bacteria != null && _renderer != null)
        {
            DrawOutline();
        }
	}

    public void Clear()
    {}
}