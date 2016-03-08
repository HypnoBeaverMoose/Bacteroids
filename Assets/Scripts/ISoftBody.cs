using UnityEngine;
using System.Collections;

public interface ISoftBody 
{
    void Init();

    void UpdateBody();

    void UpdateDeformation();

    void AddDeformingForce(Vector3 point, Vector3 force);

}
