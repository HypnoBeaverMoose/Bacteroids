using UnityEngine;
using System.Collections;

public static class SoftBodyHelper
{
    public static SpringJoint2D CreateSpringJoint(GameObject obj, Rigidbody2D anchor, float frequency, float damping, Vector2 connectedAnchor)
    {
        var spring = obj.AddComponent<SpringJoint2D>();
        ConfigureSpringJoint(spring, anchor, frequency, damping, connectedAnchor);
        return spring;
    }

    public static SliderJoint2D CreateSliderJoint(GameObject obj, Rigidbody2D anchor, float minDistance, float maxDistance)
    {
        var slider = obj.AddComponent<SliderJoint2D>();
        slider.connectedBody = anchor;
        slider.autoConfigureAngle = false;
        slider.autoConfigureConnectedAnchor = true;
        slider.useLimits = true;
        var limits = slider.limits;
        limits.min = minDistance;
        limits.max = maxDistance;
        slider.limits = limits;
        return slider;
    }

    public static void ConfigureSpringJoint(SpringJoint2D spring, Rigidbody2D anchor, float frequency, float damping, Vector2 connectedAnchor)
    {
        spring.autoConfigureDistance = true;
        spring.autoConfigureConnectedAnchor = false;
        spring.connectedBody = anchor;
        spring.connectedAnchor = connectedAnchor;
        spring.frequency = frequency;
        spring.dampingRatio = damping;
        spring.autoConfigureDistance = false;
    }

    public static Node CreateNode(GameObject go, Transform parent, Vector3 position, Rigidbody2D prototype, float radius, bool setPos = true)
    {
        var body = CreateRigidChild(go, parent, position, prototype, radius, setPos);
        var node = body.gameObject.AddComponent<Node>();
        //node.Init(body, body.GetComponent<CircleCollider2D>());
        return node;
    }

    public static Rigidbody2D CreateRigidChild(GameObject go, Transform parent, Vector3 position, Rigidbody2D prototype, float radius, bool setPos = true)
    {
        go.layer = LayerMask.NameToLayer("Bacteria");
        go.AddComponent<CircleCollider2D>().radius = radius;
        if (setPos)
        {
            var tr = go.transform;
            tr.SetParent(parent, true);
            tr.localPosition = position;
            tr.rotation = Quaternion.identity;
            tr.localScale = Vector3.one;
        }
        var body = go.AddComponent<Rigidbody2D>();
        body.mass = prototype.mass;
        body.drag = prototype.drag;
        body.angularDrag = prototype.angularDrag;
        body.gravityScale = prototype.gravityScale;
        body.interpolation = prototype.interpolation;
        body.constraints = prototype.constraints;
        body.collisionDetectionMode = prototype.collisionDetectionMode;
        go.AddComponent<CollisionHandler>();
        return body;
    }
}
