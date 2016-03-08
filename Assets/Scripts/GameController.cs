using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameController : MonoBehaviour {


    //public int WaveNumber { get; private set; }
    //public int StartEnemies = 1;
    //public int SpawnTime = 60;
    //public int EnemyInc = 5;
    //public int WaveTimeout = 3;


    
    //public delegate void WaveEndEvent(int timeout);
    //public static event WaveEndEvent OnWaveEnd;

    //private int _enemiesToSpawn;
    //private float _spawnTimer;   
    //private bool _waveStarted = false;
    public float Score { get; set; }
    public float Radius { get { return _radius; } }

    [SerializeField]
    private EndScreen _endScreen;
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

    private List<GameObject> _enemies = new List<GameObject>();
    private Player _player;
    private float _spawnTimer = 0;
    private bool _stopSpawn = false;

	void Awake () 
    {
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

    private void StartGame()
    {
        _player = Instantiate<GameObject>(_playerPrefab).GetComponent<Player>();
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

    private void PlayerKilled()
    {
        _stopSpawn = true;
       
        Destroy(_player.gameObject);
        _endScreen.gameObject.SetActive(true);
    }

    public GameObject SpawnEnemy(Vector2 position)
    {
        var go = (GameObject)Instantiate(_enemyPrefab, position, Quaternion.identity);
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
            if (_spawnTimer == 0 || _enemies.Count == 0)
            {
                _enemies.Add(SpawnEnemy(FindSpawnPos()));
                _spawnTimer = GetSpawnTime();
            }
        }
    }

    //private IEnumerator EndWaveCoroutine(int Timeout)
    //{
    //    yield return new WaitForSeconds(Timeout);
    //    StartWave(++WaveNumber);
    //}

    //public void StartWave(int wave)
    //{
    //    _enemiesToSpawn = StartEnemies + (wave - 1) * EnemyInc;
    //    _enemies.Clear();
    //    _spawnTimer = 0;
    //    _waveStarted = true;
    //    //OnWaveStart(wave);
    //}

    //public void EndWave()
    //{
    //    _waveStarted = false;
    //    _player.transform.position = Vector2.zero;
    //    StartCoroutine(EndWaveCoroutine(WaveTimeout));
    //    //OnWaveEnd(WaveTimeout);
    //}


}
