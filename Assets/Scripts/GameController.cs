using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class GameController : MonoBehaviour 
{

    public float Score { get; set; }
    public float Radius { get { return _radius; } }

    [SerializeField]
    private Color[] _colors;

    [SerializeField]
    private EndScreen _endScreen;
    [SerializeField]
    private HighScoreScreen _scoreScreen;
    [SerializeField]
    private  StartScreen _startScreen;
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private AnimationCurve _spawnCurve;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _scorePerKill;
    [SerializeField]
    private float _startBacteriaSize;
    [SerializeField]
    private int _startBacteriaVertices;

    private List<GameObject> _enemies = new List<GameObject>();
    private Player _player;
    private float _spawnTimer = 0;
    private bool _stopSpawn = false;

	void Awake () 
    {
        _startScreen.gameObject.SetActive(true);
        _startScreen.OnStartGame += StartGame;
        _endScreen.OnEndGame += EndGame;
        _scoreScreen.OnSkipScores += SkipScores;
        _stopSpawn = true;
        HighScores.Load();
    }

    void SkipScores()
    {
        _scoreScreen.gameObject.SetActive(false);
        _startScreen.gameObject.SetActive(true);
    }

    void EndGame()
    {
        _endScreen.gameObject.SetActive(false);
        _scoreScreen.gameObject.SetActive(true);
    }

    private void StartGame()
    {
        _player = Instantiate<GameObject>(_playerPrefab).GetComponent<Player>();
        _player.OnColorChanged+= OnColorChanged;
        _player.OnPlayerKilled += PlayerKilled;
        _stopSpawn = false;
        _spawnTimer = GetSpawnTime();
        
        var energies = FindObjectsOfType<Energy>();
        foreach (var energy in energies)
        {
            Destroy(energy.gameObject);
        }
        foreach (var enemy in _enemies)
        {
            Destroy(enemy);
        }
        _enemies.Clear();
        Score = 0;        
    }

    private void OnColorChanged(Color newColor)
    {
        foreach (var item in GetComponentsInChildren<SpriteRenderer>())
        {
            item.color = new Color(newColor.r, newColor.g, newColor.b, item.color.a);
        }
    }

    private void PlayerKilled()
    {
        _stopSpawn = true;
       
        Destroy(_player.gameObject);
        _endScreen.gameObject.SetActive(true);
    }

    public GameObject SpawnBacteria(Vector2 position, float size, int vertices, Color color)
    {
        var go = (GameObject)Instantiate(_enemyPrefab, position, Quaternion.identity);
        go.GetComponent<Bacteria>().Color = color;
        var body = go.GetComponent<CompoundSoftBody>();
        body.Size = size;
        body.Vertices = vertices;
        _enemies.Add(go);
        return go;
    }

    private Vector2 FindSpawnPos()
    {
        var pos =  Random.insideUnitCircle * _radius;
        while (Vector2.Distance(pos, _player.transform.position) < 2.0f)
        {
            pos = Random.insideUnitCircle * _radius; ;
        }

        return pos;
    }

    private float GetSpawnTime()
    {
        return _spawnCurve.Evaluate(Time.time);
    }

    private Color GetSpawnColor()
    {
        return Random.value < 0.3f ? Color.white : _colors[Random.Range(0, _colors.Length)];
    }

	// Update is called once per frame
	void Update () 
    {
        if (!_stopSpawn)
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i] == null)
                {
                    _enemies.RemoveAt(i--);
                    Score += _scorePerKill;
                }
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 || _enemies.Count == 0)
            {
                SpawnBacteria(FindSpawnPos(), _startBacteriaSize, 4, GetSpawnColor());
                _spawnTimer = GetSpawnTime();
            }
        }
    }
}
