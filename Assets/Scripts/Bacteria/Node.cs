using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public const float hitFrequency = 0;

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
        public T GetJoint<T>() where T : class { return Joint as T; }
    }
    public bool EditMode = false;


    public float Damping;
    public float Frequency;

    public float PivotFrequency;
    public float PivotDamping;

    public float MinDistance;
    public float MaxDistance;

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

    public SliderJoint2D GetSlider()
    {
        return _sliders[0].GetJoint<SliderJoint2D>();
    }

    private void Start()
    {
        _transform = transform;
    }
        
    public void ConnectSlider(JointType type, Node other)
    {
        Connect(_sliders, type, other, SoftBodyHelper.CreateSliderJoint(gameObject, other.Body, MinDistance, MaxDistance));
    }

    public void Disconnect()
    {
        for (JointType type = JointType.Center; type < JointType.TypeLength; type++)
        {
            Disconnect(type);
        }
    }

    public void Disconnect(JointType type)
    {
        if (_springs.ContainsKey(type))
        {
            _springs[type].Joint.connectedBody = null;
            _springs[type].Joint.enabled = false;
        }
        if (_sliders.ContainsKey(type))
        {
            _sliders[type].Joint.connectedBody = null;
            _sliders[type].Joint.enabled = false;
        }
    }

    public void ConnectSpring(JointType type, Node other)
    {
        Connect(_springs,type, other, SoftBodyHelper.CreateSpringJoint(gameObject, other.Body, type == JointType.Center ? PivotFrequency : Frequency, type == JointType.Center ? PivotDamping : Damping));
    }

    private static void Connect(Dictionary<JointType, JointNode> dict, JointType type, Node other, Joint2D joint)
    {
        if (dict.ContainsKey(type))
        {
            DestroyImmediate(dict[type].Joint);
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
//                if (_springs.ContainsKey(type))
//                {
//                    float distance = Vector2.Distance(_rigidbody.position, _springs[type].Node.Body.position);
//
//                    if (distance < _springs[type].GetJoint<SpringJoint2D>().distance * 0.5f || distance > _springs[type].GetJoint<SpringJoint2D>().distance * 1.5f)
//                    {
//                        _springs[type].GetJoint<SpringJoint2D>().frequency = hitFrequency;
//                        if (distance < _springs[type].GetJoint<SpringJoint2D>().distance * 0.2f)
//                        {
//                            if (OnNodeUnstable != null)
//                            {
//                                OnNodeUnstable(this);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        _springs[type].GetJoint<SpringJoint2D>().frequency = type == JointType.Center ? PivotFrequency : Frequency;
//                    }
//                }
//            }
//        }
//        if (EditMode)
//        {
//            for (JointType type = JointType.Center; type < JointType.TypeLength; type++)
//            {
//                if (_springs.ContainsKey(type))
//                {
//                    _springs[type].GetJoint<SpringJoint2D>().frequency = type == JointType.Center ? PivotFrequency : Frequency;
//                    _springs[type].GetJoint<SpringJoint2D>().dampingRatio = type == JointType.Center ? PivotDamping : Damping;
//                }
//
//                if (_sliders.ContainsKey(type))
//                {
//                    var limits = _sliders[type].GetJoint<SliderJoint2D>().limits;
//                    limits.max = MaxDistance;
//                    limits.min = MinDistance;
//                    _sliders[type].GetJoint<SliderJoint2D>().limits = limits;
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
