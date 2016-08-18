using UnityEngine;
using System.Collections;

enum SpawnStrategy {SpawnAvoid, SpawnGrid };

public interface ISpawnStrategy 
{
    Vector2 GetSpawnPosition(Bacteria[] enemies);
}
