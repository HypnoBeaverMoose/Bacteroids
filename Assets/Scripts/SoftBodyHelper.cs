using UnityEngine;
using System.Collections;

public static class SoftBodyHelper
{
    public static SpringJoint2D CreateSpringJoint(GameObject obj, Rigidbody2D anchor, float frequency, float damping)
    {
        var spring = obj.AddComponent<SpringJoint2D>();
        spring.connectedBody = anchor;
        spring.distance = (obj.transform.position - anchor.transform.position).magnitude;
        spring.frequency = frequency;
        spring.dampingRatio = damping;
        spring.autoConfigureConnectedAnchor = true;
        spring.autoConfigureDistance = false;
        return spring;
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
        go.AddComponent<CollisionHandler>();
        return body;
    }
}
