using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    public delegate void CollisionEvent(Collision2D collision, Node node);

    public delegate void TriggerEvent(Collider2D other, Node node);

    public event Action<Node> OnNodeUnstable;
    public event CollisionEvent OnCollisionEnter;
    public event TriggerEvent OnTriggerEnter;
    public event TriggerEvent OnTriggerExit;

    private List<SpringJoint2D> _joints = new List<SpringJoint2D>();
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private CircleCollider2D _collider;
    [SerializeField]
    private SpringJoint2D _joint;
    [SerializeField]

    public int JointsCount { get { return _joints.Count; } }
    public float Frequency { get { return _joint.frequency; } set { _joint.frequency = value; } }
    public float Damping { get { return _joint.dampingRatio; } set { _joint.dampingRatio = value; } }
    public Vector2 TargetPosition { get { return _joint.connectedAnchor; } set { _joint.connectedAnchor = value; } }
    public Rigidbody2D TargetBody { get { return _joint.connectedBody; } set { _joint.connectedBody = value; } }

    public SpringJoint2D TargetJoint { get { return _joint; } }
    public SpringJoint2D this[int index] { get { return _joints[index]; } }
    public Rigidbody2D Body { get { return _rigidbody; } }
    public CircleCollider2D Collider { get { return _collider; } }

    public float Health = 1.0f;

    private void Start()
    {
    }

    public void ClearEvents()
    {
        OnCollisionEnter = null;
        OnTriggerEnter = null;
        OnTriggerExit = null;
    }

    public void Disconnect()
    {
        foreach (var joint in _joints)
        {
            Disconnect(joint);
        }
    }

    public void Disconnect(SpringJoint2D joint)
    {
        if (_joints.Contains(joint))
        {
            joint.connectedBody = null;
            joint.enabled = false;
        }
        else
        {
            Debug.LogError("Trying to disconnect a joint that is not a part of this node");
        }
    }

    public Joint2D ConnectSpring(Node other, Vector2 connectedAnchor, float frequency, float damping)
    {
        SpringJoint2D joint = _joints.Find(jnt => jnt.connectedBody == other.Body);

        if (joint == null)
        {
            joint = _joints.Find(jnt => !jnt.enabled);
        }

        if (joint != null)
        {
            joint.enabled = true;
            SoftBodyHelper.ConfigureSpringJoint(joint, other.Body, frequency, damping, connectedAnchor);
            return joint;
        }

        joint = SoftBodyHelper.CreateSpringJoint(gameObject, other.Body, frequency, damping, connectedAnchor);
        _joints.Add(joint);
        return joint;
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
//    public struct JointNode
//    {
//        public Joint2D Joint;
//        public Node Node;
//        public T GetJoint<T>() where T : class { return Joint as T; }
//    }
//    public bool EditMode = false;
//    [SerializeField]
//    private bool _constrain = false;
//    private Dictionary<JointType, JointNode> _springs = new Dictionary<JointType, JointNode>();
//    private Dictionary<JointType, JointNode> _sliders = new Dictionary<JointType, JointNode>();
//    public const float hitFrequency = 0;
//    public enum JointType
//    {
//        Center = 0,
//        Left,
//        Right,
//        TypeLength
//
//    }
//    private float _damping;
//    [SerializeField]
//    private float _frequency;
//
//    public float Damping
//    {
//        get
//        {
//            return _damping;
//        }
//        set
//        {
//            _damping = value;
//            foreach (var joint in _joints)
//            {
//                joint.dampingRatio = _damping;
//            }
//        }
//    }
//    public float Frequency
//    {
//        get
//        {
//            return _frequency;
//        }
//        set
//        {
//            _frequency = value;
//            foreach (var joint in _joints)
//            {
//                joint.frequency = _frequency;
//            }
//        }
//    }
//
//    public float PivotFrequency;
//    public float PivotDamping;
//
//    public float MinDistance;
//    public float MaxDistance;
//    public SliderJoint2D GetSlider()
//    {
//        return _sliders[0].GetJoint<SliderJoint2D>();
//    }
//    private static void Connect(Dictionary<JointType, JointNode> dict, JointType type, Node other, Joint2D joint)
//    {
//        if (dict.ContainsKey(type))
//        {
//            DestroyImmediate(dict[type].Joint);
//            dict.Remove(type);
//        }
//        var jointNode = new JointNode();
//        jointNode.Node = other;
//        jointNode.Joint = joint;
//        dict.Add(type, jointNode);
//    }
//    public void ConnectSlider(JointType type, Node other)
//    {
//        Connect(_sliders, type, other, SoftBodyHelper.CreateSliderJoint(gameObject, other.Body, MinDistance, MaxDistance));
//    }
//    private void FixedUpdate()
//    {
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
//    }