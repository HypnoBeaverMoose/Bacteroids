using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaDrawer : MonoBehaviour 
{
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            foreach (var shell in _shells)
            {
                shell.Color = _color;
            }
        }
    }
            
    private Bacteria _bacteria;
    [SerializeField]
    private Color _color = new Color();
    [SerializeField]
    private GameObject _attachablePrefab;
    [SerializeField]
    private ShellDrawer[] _shells;

    private void Awake()
    {              
        if (_bacteria != null)
        {
            foreach (var shell in _shells)
            {
                shell.Init(_bacteria.GetNodes());
            }
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
        foreach (var shell in _shells)
        {
            shell.Init(bacteria.GetNodes());
        }
    }

    public void Clear()
    {
        foreach (var shell in _shells)
        {
            shell.Clear();
        }
    }
}