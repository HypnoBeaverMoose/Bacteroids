using UnityEngine;
using System.Collections;

public class SpawnStrategyAvoid : ISpawnStrategy
{ 
    private Camera _camera;
    private int _iterations;
    public SpawnStrategyAvoid(Camera camera, int iterations)
    {
        _camera = camera;
        _iterations = iterations;
    }

    public Vector2 GetSpawnPosition(Bacteria[] enemies)
    {
        int iterations = _iterations;
        Vector2 pos = Vector2.zero;
        do
        {
            pos = new Vector2(Random.Range(-1.0f, 1.0f) * _camera.orthographicSize * _camera.aspect, Random.Range(-1.0f, 1.0f) * _camera.orthographicSize);
            foreach (var bacteria in enemies)
            {
                if (Vector2.Distance((Vector2)bacteria.transform.position, pos) > bacteria.Radius * 4)
                {
                    break;
                }
            }
        }
        while (iterations-- > 0);
        return pos;
    }
}