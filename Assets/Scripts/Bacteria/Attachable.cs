using UnityEngine;

public class Attachable : MonoBehaviour
{
    [SerializeField]
    private float _pulseAmount = 0.2f;
    [SerializeField]
    private float _pulseSpeed = 5;


    private Vector2 _anchoredPosition;
    private Rigidbody2D _anchor;
    private Vector2 _smoothDampVelocity;
    private Vector3 _originalScale;
    private float _pulseOffset = 0;

    private void Start()
    {
        transform.localScale *= 0.5f + Random.value * 1.5f;
        _originalScale = transform.localScale;
        _pulseOffset = Random.value * 100;
    }


    private void FixedUpdate()
    {
        if (_anchor != null)
        {
            transform.position = (Vector2)_anchor.transform.TransformPoint(_anchoredPosition);
        }
        if (_anchor == null)
        {
            Destroy(gameObject);
        }
        transform.localScale = _originalScale * (1.0f - _pulseAmount) + _originalScale * _pulseAmount * Mathf.Sin(Time.unscaledTime * _pulseSpeed + _pulseOffset);
    }

    public void AttachTo(Rigidbody2D body, Vector2 anchorPosition)
    {
        _anchor = body;
        _anchoredPosition = anchorPosition;
    }
}
