using UnityEngine;
using System.Collections;

enum SpawnStrategy {SpawnAvoid, SpawnGrid };

public interface ISpawnStrategy 
{
    bool GetSpawnPosition(Bacteria[] enemies, Player player, out Vector2 position);
}
