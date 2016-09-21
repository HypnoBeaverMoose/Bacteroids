using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController : MonoBehaviour 
{
    public static GameController Instance 
    {
        get
        { 
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameController>();
                if (_instance == null)
                {
                    var go = new GameObject("GameController");
                    _instance = go.AddComponent<GameController>();
                }
            }
            return _instance;
        }
    }
    private static GameController _instance = null;

    public int Lives { get; private set; }
    public float Score { get; set; }

    [SerializeField]
    private Color[] _colors;
    [SerializeField]
    private SpawnStrategy _spawnType;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private EndScreen _endScreen;
    [SerializeField]
    private HighScoreScreen _scoreScreen;
    [SerializeField]
    private  StartScreen _startScreen;
    [SerializeField]
    private GameObject _bacteriaPrefab;
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private AnimationCurve _spawnCurve;
    [SerializeField]
    private float _scorePerKill;
    [SerializeField]
    private int _startBacteriaVertices;
    [SerializeField]
    private int _maxBacterias;
    [SerializeField]
    private int _startLives;

    private Player _player;
    private float _spawnTimer = 0;
    private bool _stopSpawn = false;
    private ISpawnStrategy _strategy;
    private SplitController _spawn;
    public SplitController Spawn { get { return _spawn; } }

	void Awake () 
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }       
        _spawn = gameObject.GetComponent<SplitController>();

        if (_spawnType == SpawnStrategy.SpawnAvoid)
        {
            _strategy = new SpawnStrategyAvoid(_camera, 10);
        }
        else
        {
            _strategy = new SpawnStrategyGrid(_camera, 5,5, 10);
        }

        _startScreen.gameObject.SetActive(true);
        _startScreen.OnStartGame += StartGame;
        _endScreen.OnEndGame += EndGame;
        _scoreScreen.OnSkipScores += SkipScores;
        _spawn.OnBacteriaKilled += EnemyKilled;
        _stopSpawn = true;

        Lives = _startLives;
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
        
        var energies = FindObjectsOfType<Energy>();
        foreach (var energy in energies)
        {
            Destroy(energy.gameObject);
        }
        var enemies = FindObjectsOfType<Bacteria>();
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
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
        _player.OnPlayerKilled += PlayerKilled;
        Destroy(_player.gameObject);
        if (Lives == 0)
        {
            _stopSpawn = true;       
            _endScreen.gameObject.SetActive(true);
        }
        else
        {
            _player = Instantiate<GameObject>(_playerPrefab).GetComponent<Player>();
            _player.OnPlayerKilled += PlayerKilled;
            Lives--;
        }
    }

    private void EnemyKilled()
    {
        Score += _scorePerKill;
    }

    private Vector2 GetSpawnPosition(Bacteria[] enemies)
    {        
        return _strategy.GetSpawnPosition(enemies);
    }

    private float GetSpawnTime()
    {
        return 20.0f;// _spawnCurve.Evaluate(Time.time);
    }

    public Color GetRandomColor()
    {
        return _colors[Random.Range(0, _colors.Length)];
    }

	void Update () 
    {
        if (!_stopSpawn)
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 )
            {
                var enemies = FindObjectsOfType<Bacteria>();
                if (enemies.Length < _maxBacterias)
                {
                    Spawn.SpawnBacteria(GetSpawnPosition(enemies));
                }
                _spawnTimer = GetSpawnTime();
            }

        }
    }
}
