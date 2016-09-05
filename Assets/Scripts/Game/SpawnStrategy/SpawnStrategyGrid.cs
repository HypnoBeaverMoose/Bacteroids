using UnityEngine;
using System.Collections;

public class SpawnStrategyGrid : ISpawnStrategy
{ 
    private Camera _camera;
    private int[,] _grid;
    private int _iterations;

    public SpawnStrategyGrid(Camera camera, int gridSizeX, int gridSizeY, int iterations)
    {
        _camera = camera;
        _grid = new int[gridSizeX, gridSizeY];
        _iterations = iterations;
    }

    public Vector2 GetSpawnPosition(Bacteria[] enemies)
    {
        float sizeX = _camera.orthographicSize * _camera.aspect;
        float sizeY = _camera.orthographicSize;
        ClearGrid();
        foreach (var enemy in enemies)
        {
            int x = Mathf.FloorToInt(enemy.transform.position.x / sizeX);
            int y = Mathf.FloorToInt(enemy.transform.position.y / sizeY);
            _grid[x, y]++;
        }
        int min = _grid[0, 0]; int minX = 0, minY = 0;

        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for(int y = 0; y < _grid.GetLength(0); y++)
            {
                if (_grid[x, y] < min)
                {
                    minX = x;
                    minY = y;
                }
            }
        }

        return GetPositionInGrid(minX, minY, _grid[minX, minY], enemies);
    }
    private Vector2 GetPositionInGrid(int x, int y, int count, Bacteria[] enemies)
    {        
        Vector2 size = new Vector2(((_camera.orthographicSize * _camera.aspect) / (float)_grid.GetLength(0)), ((_camera.orthographicSize) / (float)_grid.GetLength(1)));
        Vector2 offset = new Vector2(size.x * x, size.y * y);
        Vector2 pos = new Vector2(offset.x + Random.value * size.x, offset.y + Random.value * size.y);

        if (count == 0)
        {
            return pos;
        }
        int iterations = _iterations;
        while (iterations-- > 0)
        {
            foreach (var bacteria in enemies)
            {
                if (Vector2.Distance(bacteria.transform.position, pos) > bacteria.Radius * 4)
                {
                    break;
                }
            }
            pos = new Vector2(offset.x + Random.value * size.x, offset.y + Random.value * size.y);
        }

        return pos;
    }

    private void ClearGrid()
    {
        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for(int y = 0; y < _grid.GetLength(0); y++)
            {
                _grid[x, y] = 0;
            }
        }
    }
}