using UnityEngine;
using System.Collections;

public class Wrappable : MonoBehaviour 
{
    
    private float radius = 0.0f;
    public float Size { get { return radius; } set {radius = value; } }

    private Vector3 wrap = Vector3.zero;
    private bool shouldWrap = false;
    public Vector3 Position 
    { 
        get 
        { 
            return shouldWrap ? wrap  : (Vector3)_rigidbody.position;
        } 
        set  
        {   
            wrap = value;
            shouldWrap = true;
        } 
    }
    private Rigidbody2D _rigidbody;
    private ScreenWrapper wrapper;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        wrapper = FindObjectOfType<ScreenWrapper>();
        if (wrapper != null)
        {
            wrapper.RegisterWrappable(this);
        }
        else
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (shouldWrap)
        {
            var offset = (Vector2)wrap - _rigidbody.position;
            _rigidbody.position = wrap;
            foreach (var child in GetComponentsInChildren<Rigidbody2D>())
            {
                if (!child.Equals(_rigidbody))
                {
                    child.position += offset;
                }
            }
            shouldWrap = false;
        }
    }

}
