using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public const float hitFrequency = 10;

    public enum JointType
    {
        Center = 0,
        Left,
        Right,
        TypeLength

    }

    public delegate void CollisionEvent(Collision2D collision,Node node);

    public delegate void TriggerEvent(Collider2D other,Node node);

    public event Action<Node> OnNodeUnstable;
    public event CollisionEvent OnCollisionEnter;
    public event TriggerEvent OnTriggerEnter;
    public event TriggerEvent OnTriggerExit;

    public struct JointNode
    {
        public Joint2D Joint;
        public Node Node;
    }

    [SerializeField]
    private float _damping;
    [SerializeField]
    private float _frequency;
    [SerializeField]
    private float _minDistance;
    [SerializeField]
    private float _maxDistance;

    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private CircleCollider2D _collider;
    [SerializeField]
    private bool _constrain = false;

    public JointNode this [JointType type] { get { return _springs[type]; } }

    public Rigidbody2D Body { get { return _rigidbody; } }

    public CircleCollider2D Collider { get { return _collider; } }

    private Transform _transform;
    private Dictionary<JointType, JointNode> _springs = new Dictionary<JointType, JointNode>();
    private Dictionary<JointType, JointNode> _sliders = new Dictionary<JointType, JointNode>();

    private void Start()
    {
        _transform = transform;
    }

    public void ConnectSlider(JointType type, Node other)
    {
        Connect(_sliders, type, other, SoftBodyHelper.CreateSliderJoint(gameObject, other.Body, _minDistance, _maxDistance));
    }

    public void ConnectSpring(JointType type, Node other)
    {
        Connect(_springs,type, other, SoftBodyHelper.CreateSpringJoint(gameObject, other.Body, _frequency, _damping));
    }

    private static void Connect(Dictionary<JointType, JointNode> dict, JointType type, Node other, Joint2D joint)
    {
        if (dict.ContainsKey(type))
        {
            Destroy(dict[type].Joint);
            dict.Remove(type);
        }
        var jointNode = new JointNode(); 
        jointNode.Node = other;
        jointNode.Joint = joint;
        dict.Add(type, jointNode);
    }

    private void FixedUpdate()
    {
//        if (_constrain)
//        {
//            for (JointType type = JointType.Center; type < JointType.TypeLength; type++)
//            {
//                if (_nodes.ContainsKey(type))
//                {
//                    float distance = Vector2.Distance(_rigidbody.position, _nodes[type].Node.Body.position);
//
//                    if (distance < _nodes[type].Joint.distance * 0.5f || distance > _nodes[type].Joint.distance * 1.5f)
//                    {
//                        _nodes[type].Joint.frequency = hitFrequency;
//                        if (distance < _nodes[type].Joint.distance * 0.2f)
//                        {
//                            if (OnNodeUnstable != null)
//                            {
//                                OnNodeUnstable(this);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        _nodes[type].Joint.frequency = _frequency;
//                    }
//                }
//            }
//        }
    }

    #region Collision Handlers

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (OnCollisionEnter != null)
        {
            OnCollisionEnter(collision, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (OnTriggerEnter != null)
        {
            OnTriggerEnter(other, this);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (OnTriggerExit != null)
        {
            OnTriggerExit(other, this);
        }
    }

    #endregion
}
