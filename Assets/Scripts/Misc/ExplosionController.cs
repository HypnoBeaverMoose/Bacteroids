using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionController : MonoBehaviour
{
    public enum ExplosionType { Big, Small, Random }
    [System.Serializable]
    public class Explosion
    {
        public ExplosionType Type;
        public GameObject Prefab;
    }

    [SerializeField]
    private Explosion[] _explosionsTypes;
    [SerializeField]
    private float KeepTime;

    private static ExplosionController _intance = null;
    public static ExplosionController Instance
    {
        get
        {
            if (_intance == null)
            {
                _intance = FindObjectOfType<ExplosionController>();
                if (_intance == null)
                {
                    var go = new GameObject("Explosions");
                    _intance = go.AddComponent<ExplosionController>();
                }
            }
            return _intance;
        }
    }

    private Dictionary<ExplosionType, GameObject> _explosions;
    // Use this for initialization
    void Awake()
    {
        if (_intance == null)
        {
            _intance = this;
            _explosions = new Dictionary<ExplosionType, GameObject>();
            foreach (var exp in _explosionsTypes)
            {
                _explosions.Add(exp.Type, exp.Prefab);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnExplosion(ExplosionType type, Vector2 position, Color color)
    {
        GameObject go;
        if (_explosions.TryGetValue(type, out go))
        {
            var pa = ((GameObject)Instantiate(go, position, Quaternion.identity)).GetComponent<ParticleSystem>();
            pa.startColor = color;
            pa.Emit(1);
        }
    }
}
