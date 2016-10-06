using UnityEngine;
using System.Collections;

public class SpawnStrategyAvoid : ISpawnStrategy
{
    private const float minBacteriaDistance = 1.5f;
    private const float minPlayerDistance = 3;
    private Camera _camera;
    private int _iterations;
    public SpawnStrategyAvoid(Camera camera, int iterations)
    {
        _camera = camera;
        _iterations = iterations;
    }

    public bool GetSpawnPosition(Bacteria[] enemies, Player player, out Vector2 position)
    {
        int iterations = _iterations;
        position = Vector2.zero;
        do
        {
            position = new Vector2(Random.Range(-1.0f, 1.0f) * _camera.orthographicSize * _camera.aspect,
                                                    Random.Range(-1.0f, 1.0f) * _camera.orthographicSize);
            if (player == null || Vector2.Distance((Vector2)player.transform.position, position) > minPlayerDistance)
            {
                bool farEnough = true;
                foreach (var bacteria in enemies)
                {
                    if (Vector2.Distance((Vector2)bacteria.transform.position, position) < minBacteriaDistance)
                    {
                        farEnough = false;
                        break;
                    }
                }
                if (farEnough)
                {
                    break;
                }
            }
        }
        while (iterations-- > 0);
        return iterations > 0;
    }
}