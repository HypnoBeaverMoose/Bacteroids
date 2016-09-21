using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ScreenWrapper : MonoBehaviour 
{

    public float Offset = 0.1f;
    private Camera _cam;
    private Rect _limits;
    private List<Wrappable> wrappables = new List<Wrappable>();

	void Start () 
    {
        _cam = FindObjectOfType<Camera>();
        _limits = new Rect( _cam.transform.position.x - _cam.orthographicSize * _cam.aspect,
                            _cam.transform.position.y - _cam.orthographicSize, 
                            2 * _cam.orthographicSize * _cam.aspect, 
                            2 *_cam.orthographicSize
                          );
	}
	
    public void RegisterWrappable(Wrappable obj)
    {
        wrappables.Add(obj);
    }
        
	void Update () 
    {
        for (int i = 0; i < wrappables.Count; i++)
        {
            if (wrappables[i] == null)
            {
                wrappables.RemoveAt(i--);
            }
        }

        for (int i = 0; i < wrappables.Count; i++)
        {
            float radius = 0;
            Vector3 position = wrappables[i].Position;
            if (_limits.yMin - position.y > (radius + Offset))
            {
                position.y = _limits.yMax + radius;
            }
            else if (position.y - _limits.yMax > (radius + Offset))
            {
                position.y = _limits.yMin - radius;
            }
            if (_limits.xMin - position.x > (radius + Offset))
            {
                position.x = _limits.xMax + radius;
            }
            else if (position.x - _limits.xMax > (radius + Offset))
            {
                position.x = _limits.xMin - radius;
            }
            wrappables[i].Position = position;
        }
    }
}
