﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BacteriaDrawer : MonoBehaviour 
{
    public Color Color
    {
        get { return _color; }
        set
        {
            _dimColor = false;
            _color = value;        
            foreach (var shell in _shells)
            {
                if (shell != null)
                {
                    shell.Color = _color;
                }
            }
        }
    }
    private bool _dimColor;
    private Bacteria _bacteria;
    [SerializeField]
    private Color _color = new Color();
    [SerializeField]
    private float _dimAmount;
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

    
    private void Update()
    {
        if( GameController.Instance.Player != null)
        {
            if((GameController.Instance.Player.Color == Color.white || GameController.Instance.Player.Color == _color) && _dimColor)
            {
                _dimColor = false;
                foreach (var shell in _shells)
                {
                    shell.Color = _color;
                }
            }
            else if ((GameController.Instance.Player.Color != Color.white && GameController.Instance.Player.Color != _color) && !_dimColor)
            {
                _dimColor = true;
                foreach (var shell in _shells)
                {
                    float h, s, v;
                    Color.RGBToHSV(_color, out h, out s, out v);
                    shell.Color = Color.HSVToRGB(h, s * _dimAmount, v * _dimAmount);
                }
            }
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