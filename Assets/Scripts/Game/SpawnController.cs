using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnController : MonoBehaviour 
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private float _energyInitialForce;
    [SerializeField]
    private GameObject _bacteriaPrefab;
    [SerializeField]
    private GameObject _energyPrefab;
    [SerializeField]
    private GameObject _pariclePrefab;
    [Space(20)]
    [SerializeField]
    private int _initialSpawn;
    [SerializeField]
    private float _spawnInterval;
    [SerializeField]
    private int _minBacteria;
    [SerializeField]
    private float _maxBacteria;
    [SerializeField]
    private SpawnStrategy _spawnType;

    private ISpawnStrategy _strategy;
    private List<Bacteria> _enemies = new List<Bacteria>();
    private Player _player;
    private PropertyController _sampler;
    private int _kills = 0;
    public bool CanSpawn { get { return _enemies.Count < _maxBacteria; } }
    public Bacteria[] Enemies { get { return _enemies.ToArray(); } }


    private void Awake()
    {
        if (_spawnType == SpawnStrategy.SpawnAvoid)
        {
            _strategy = new SpawnStrategyAvoid(_camera, 10);
        }
        else
        {
            _strategy = new SpawnStrategyGrid(_camera, 5, 5, 10);
        }
        _sampler = GetComponent<PropertyController>();
        Player.PlayerSpawned += OnPlayerSpawned;
        Bacteria.BacteriaKilled += OnBacteriaKilled;
    }

    public void StartSpawn()
    {
        _enemies.Clear();
        SpawnWave(_initialSpawn);
        InvokeRepeating("CheckBacteria", 1.0f, 1.0f);
        InvokeRepeating("SpawnBacteria", _spawnInterval, _spawnInterval);
    }

    public void StopSpawn()
    {
        CancelInvoke("CheckBacteria");
        CancelInvoke("SpawnBacteria");
    }

    private void CheckBacteria()
    {
        if (_enemies.Count < _minBacteria )
        {
            SpawnBacteria();
        }
    }

    private void SpawnWave(int spawn)
    {
        for (int i = 0; i < _initialSpawn; i++)
        {
            SpawnBacteria();
        }
    }

    public void Split(Bacteria bacteria, int startIndex, int endIndex)
    {
        if (!CanSpawn)
        {
            return;
        }
        bacteria.Clear();
        bacteria.Vertices *= 2;
        startIndex *= 2;
        endIndex *= 2;
        for (int i = 0; i < bacteria.Vertices; i++)
        {
            Node node = bacteria[i];
            node.Disconnect();
            node.ClearEvents();
            node.TargetBody = null;
            node.transform.SetParent(null, true);
            for (int j = 0; j < bacteria.Vertices; j++)
            {
                if (i != j)
                {
                    Physics2D.IgnoreCollision(node.Collider, bacteria[j].Collider, false);
                }
            }
        }

        List<Node> leftNodes = new List<Node>();
        List<Node> rightNodes = new List<Node>(bacteria.GetNodes());
        while (startIndex != endIndex)
        {
            var currentNode = bacteria[startIndex];
            leftNodes.Add(currentNode);
            startIndex = Indexer.GetIndex(Indexer.IndexType.After, startIndex, bacteria.Vertices);
        }

        rightNodes.RemoveAll(n => leftNodes.Contains(n));
        var newColor = GameController.Instance.GetRandomColor(bacteria.Color);
        
        var bact = SpawnBacteriaFromNodes(leftNodes, bacteria.Radius * 0.5f, bacteria.Color);
        var mutate = bact.GetComponent<BacteriaMutate>();
        mutate.TriggerMutation(_sampler.SampleMutation(_kills), newColor);
        
        var originalMutate = bacteria.GetComponent<BacteriaMutate>();
        mutate.CanMutate = originalMutate.CanMutate;
        _enemies.Add(bact);

        bact = SpawnBacteriaFromNodes(rightNodes, bacteria.Radius * 0.5f, bacteria.Color);        
        bact.GetComponent<BacteriaMutate>().TriggerMutation(_sampler.SampleMutation(_kills), newColor);
        bact.GetComponent<BacteriaMutate>().CanMutate = bacteria.GetComponent<BacteriaMutate>().CanMutate;
        _enemies.Add(bact);
        bacteria.Kill();
    }

    public Bacteria SpawnBacteriaFromNodes(List<Node> nodes, float radius, Color color)
    {
        Vector2 com = Vector2.zero;
        foreach (var node in nodes)
        {
            com += node.Body.position;
            node.gameObject.SetActive(false);
        }

        var bacteria = ((GameObject)Instantiate(_bacteriaPrefab, com / nodes.Count, Quaternion.identity)).GetComponent<Bacteria>();
        bacteria.SetNodes(nodes);
        bacteria.Color = color;
        bacteria.Radius = radius;

        return bacteria;
    }

    public Bacteria GetSpawnedBacteria()
    {
        if (!CanSpawn)
        {
            return null;
        }
        Vector2 position;
        var enemies = FindObjectsOfType<Bacteria>();
        if (enemies.Length < _maxBacteria && _strategy.GetSpawnPosition(enemies, _player, out position))
        {
            var bacteria = ((GameObject)Instantiate(_bacteriaPrefab, position, Quaternion.identity)).GetComponent<Bacteria>();
            _enemies.Add(bacteria);
            return bacteria;
        }
        return null;
    }

    public void SpawnBacteria()
    {
        if (!CanSpawn)
        {
            return;
        }
        Vector2 position;
        var enemies = FindObjectsOfType<Bacteria>();
        if (enemies.Length < _maxBacteria && _strategy.GetSpawnPosition(enemies, _player, out position))
        {
            var bacteria = ((GameObject)Instantiate(_bacteriaPrefab, position, Quaternion.identity)).GetComponent<Bacteria>();
            bacteria.GetComponent<BacteriaGrowth>().GrowthRate = _sampler.SampleGrowthRate(_kills);
            bacteria.GetComponent<BacteriaAI>().MoveTimeout = _sampler.SampleSpeed(_kills);
            bacteria.GetComponent<BacteriaMutate>().CanMutate = _sampler.CanMutate(_kills);
            _enemies.Add(bacteria);
        }
    }

    public Energy SpawnEnergy(Vector3 position, Vector3 initialDirection, float radius, Color color)
    {
        var obj = Instantiate(_energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.RadiusChange = radius;
        energy.Color = color;
        energy.GetComponent<Rigidbody2D>().AddForce(initialDirection * _energyInitialForce);
        return energy;
    }

    private void OnPlayerSpawned(Player obj)
    {
        _player = obj;
        _player.PlayerKilled += OnPlayerKilled;
    }

    private void OnPlayerKilled()
    {
        _player = null;
    }

    private void OnBacteriaKilled(Bacteria obj)
    {
        _kills++;
        Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.BacteriaDead, obj.transform.position);
        _enemies.Remove(obj);
    }

    private static void IgnoreCollision(Collider2D collider, Bacteria bacteria)
    {
        for (int i = 0; i < bacteria.Vertices; i++)
        {
            Physics2D.IgnoreCollision(collider, bacteria[i].Collider, true);
        }
    }

    private static Node FindNearestNode(Vector2 position, Vector2 direction, Bacteria bacteria)
    {
        var hit = Physics2D.Raycast(position, direction, Vector3.Distance(bacteria.transform.position, position) + bacteria.Radius * 4, LayerMask.GetMask("Bacteria"));

        if (hit.collider != null)
        {
            Node node = hit.collider.GetComponent<Node>();
            if (bacteria.Contains(node))
            {
                return node;
            }
        }
        return bacteria[Random.Range(0, bacteria.Vertices)];
    }
}