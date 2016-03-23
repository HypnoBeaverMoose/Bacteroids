using UnityEngine;
using System.Collections;

public class Node
{
    public enum JointType { Center = 0, Left, Right, TypeLength }

    public Rigidbody2D body;
    public CircleCollider2D collider;
    public Node NodeLeft;
    public Node NodeRight;
    public SpringJoint2D JointLeft;
    public SpringJoint2D JointRight;
    public SpringJoint2D JointCenter;

    public void SetBody(JointType joint, Rigidbody2D rigidBody, float frequency, float damping)
    {
        var spring = GetJoint(joint);
        if (spring == null)
        {
            SetJoint(joint, SoftBodyHelper.CreateSpringJoint(body.gameObject, rigidBody, frequency, damping));
        }
        else
        {
            SoftBodyHelper.SetSpringJointAnchor(spring, rigidBody, frequency, damping);
            spring.enabled = true;
        }
    }

    public void SetNode(JointType joint, Node node, float frequency, float damping)
    {
        if (joint == JointType.Left)
        {
            NodeLeft = node;
        }
        else if (joint == JointType.Right)
        {
            NodeRight = node;
        }
        SetBody(joint, node.body, frequency, damping);
    }
    public void ClearAll()
    {
        ClearNode(JointType.Right);
        ClearNode(JointType.Left);
        ClearNode(JointType.Center);       
    }

    public void ClearNode(JointType joint)
    {
        switch (joint)
        {
            case JointType.Left:
                NodeLeft = null; break;
            case JointType.Right:
                NodeRight = null; break;
            default:
                break;
        }

        var spring = GetJoint(joint);
        //spring.connectedBody = null;
        //spring.distance = 0;
        //spring.enabled = false;
        GameObject.DestroyImmediate(spring);
    }

    public SpringJoint2D GetJoint(JointType joint)
    {
        switch (joint)
        {
            case JointType.Center:
                return JointCenter;
            case JointType.Left:
                return JointLeft;
            case JointType.Right:
                return JointRight;
            default:
                return null;
        }
    }

    public void SetJoint(JointType joint, SpringJoint2D spring)
    {
        switch (joint)
        {
            case JointType.Center:
                JointCenter = spring; break;
            case JointType.Left:
                JointLeft = spring; break;
            case JointType.Right:
                JointRight = spring; break;
        }
    }
}
