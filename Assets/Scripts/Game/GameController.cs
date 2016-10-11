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
    public int Score { get; set; }
    public Player Player { get { return _player; } }

    [SerializeField]
    private ParticleSystem _backgroundParticles;
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
    private GameObject _playerPrefab;
    [SerializeField]
    private AnimationCurve _spawnCurve;
    [SerializeField]
    private float _initialSpawn;
    [SerializeField]
    private float _spawnInterval;
    [SerializeField]
    private int _minBacteria;
    [SerializeField]
    private int _maxBacteria;
    [SerializeField]
    private int _startLives;

    private Player _player;
    private float _spawnTimer = 0;
    private bool _stopSpawn = false;
    private ISpawnStrategy _strategy;
    private SpawnController _spawn;
    public SpawnController Spawn { get { return _spawn; } }

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
        _spawn = gameObject.GetComponent<SpawnController>();

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
        _stopSpawn = true;
    }

    void EndGame()
    {        
        _endScreen.gameObject.SetActive(false);
        _startScreen.gameObject.SetActive(true);
    }

    private void SpawnInitial()
    {
        var enemies = new List<Bacteria>();
        for(int i = 0; i < _initialSpawn; i++)
        {
            Vector2 pos;
            if (_strategy.GetSpawnPosition(enemies.ToArray(), _player, out pos))
            {
                enemies.Add(Spawn.SpawnBacteria(pos));
            }
        }
    }
    private void StartGame()
    {
        Lives = _startLives;

        _stopSpawn = false;

        var energies = FindObjectsOfType<Energy>();
        foreach (var energy in energies)
        {
            energy.Kill();
        }

        var enemies = FindObjectsOfType<Bacteria>();
        foreach (var enemy in enemies)
        {
            enemy.Kill();
        }
        _spawnTimer = GetSpawnTime();
        SpawnInitial();
        InvokeRepeating("CheckBacteria", 1.0f, 1.0f);
        Score = 0;
        Invoke("SpawnPlayer", 1.0f);
    }

    private void OnColorChanged(Color newColor)
    {
        foreach (var item in GetComponentsInChildren<SpriteRenderer>())
        {
            item.color = new Color(newColor.r, newColor.g, newColor.b, item.color.a);
        }
        _backgroundParticles.startColor = new Color(newColor.r, newColor.g, newColor.b, _backgroundParticles.startColor.a);
    }

    private void ShowEndScreen()
    {
        _endScreen.gameObject.SetActive(true);
    }

    private void PlayerKilled()
    {
        GetComponent<CameraShake>().ShakeCamera(0.1f, 0.03f);
        _player.Kill();
        if (--Lives <= 0)
        {
            _stopSpawn = true;
            if (Score > PlayerPrefs.GetInt("Best"))
            {
                PlayerPrefs.SetInt("Best", Score);
            }
            Invoke("ShowEndScreen", 2);
            CancelInvoke("CheckBacteria");
        }
        else
        {
            Invoke("SpawnPlayer", 1.0f);
        }
    }

    private void SpawnPlayer()
    {
        var colliders = Physics2D.OverlapCircleAll(_playerPrefab.transform.position, 1.0f);
        foreach (var coll in colliders)
        {
            if (coll.CompareTag("Enemy") && coll.GetComponentInParent<Bacteria>() != null)
            {
                coll.GetComponentInParent<Bacteria>().Kill();
            }
        }        
        _player = Instantiate<GameObject>(_playerPrefab).GetComponent<Player>();
        _player.OnColorChanged += OnColorChanged;
        _player.OnPlayerKilled += PlayerKilled;
        ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Big, _player.transform.position, Color.white);
    }

    private float GetSpawnTime()
    {
        return _spawnInterval;
    }

    public Color GetRandomColor(Color color)
    {
        int index = 0;
        if (color == Color.white)
        {
            return _colors[0];
        }
        for (int i = 0; i < _colors.Length; i++)
        {
            if (color == _colors[i])
            {
                index = i + 1;
                break;
            }
        }
        return _colors[index % _colors.Length];
    }

    private void CheckBacteria()
    {
        var enemies = FindObjectsOfType<Bacteria>();
        Vector2 position;
        if (enemies.Length < _minBacteria && _strategy.GetSpawnPosition(enemies, _player, out position))
        {
            Spawn.SpawnBacteria(position);
        }
    }

    void Update()
    {
        if (!_stopSpawn)
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 )
            {
                Vector2 position;
                var enemies = FindObjectsOfType<Bacteria>();
                if (enemies.Length < _maxBacteria && _strategy.GetSpawnPosition(enemies, _player, out position))
                {
                    Spawn.SpawnBacteria(position);
                }
                _spawnTimer = GetSpawnTime();
            }
        }
    }
}
