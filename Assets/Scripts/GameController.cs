using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameController : MonoBehaviour {

    public delegate void WaveStartEvent(int wave);
    public static event WaveStartEvent OnWaveStart;

    public delegate void WaveEndEvent(int timeout);
    public static event WaveEndEvent OnWaveEnd;

    public float Radius;
    private PlayerController _player;
    public int WaveNumber { get; private set; }
    public int StartEnemies = 1;
    public int SpawnTime = 60;
    public int EnemyInc = 5;
    public int WaveTimeout = 3;

    [SerializeField]
    private GameObject _enemyPrefab;
    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private int _enemiesToSpawn;
    private float _spawnTimer;   
    private bool _waveStarted = false;


	void Start () {
        WaveNumber = 0;
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        StartWave(++WaveNumber);
	}
    
    public void StartWave(int wave)
    {
        _enemiesToSpawn = StartEnemies + (wave - 1) * EnemyInc;
        _spawnedEnemies.Clear();        
        _spawnTimer = 0;
        _waveStarted = true;
        //OnWaveStart(wave);
    }

    public void EndWave()
    {
        _waveStarted = false;
        _player.transform.position = Vector2.zero;
        StartCoroutine(EndWaveCoroutine(WaveTimeout));
        //OnWaveEnd(WaveTimeout);
    }

    public GameObject SpawnEnemy(Vector2 position)
    {
        var go = (GameObject)Instantiate(_enemyPrefab, position, Quaternion.identity);
        //go.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle * 5, ForceMode2D.Impulse);
        //go.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, 10));
        return go;
    }
    public Vector2 FindSpawnPos()
    {
        var pos =  Random.insideUnitCircle * Radius;
        while (Vector2.Distance(pos, _player.transform.position) < 2.0f)
        {
            pos = Random.insideUnitCircle * Radius; ;
        }

        return pos;

    }
	// Update is called once per frame
	void Update () 
    {
        if (_waveStarted)
        {
            _spawnTimer -= Time.smoothDeltaTime;
            if (_spawnTimer < 0 && _enemiesToSpawn > 0)
            {
                _enemiesToSpawn--;

                _spawnTimer = SpawnTime / (float)(StartEnemies + (WaveNumber - 1) * EnemyInc);
//                Debug.Log(_spawnTimer);
                _spawnedEnemies.Add(SpawnEnemy(FindSpawnPos()));
            }
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (_enemiesToSpawn == 0 && enemies.Length == 0)
                    EndWave();

        }
	}

    private IEnumerator EndWaveCoroutine(int Timeout)
    {
        yield return new WaitForSeconds(Timeout);
        StartWave(++WaveNumber);
    }

}
