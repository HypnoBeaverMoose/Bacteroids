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
    private EndScreen _endScreen;
    [SerializeField]
    private HighScoreScreen _scoreScreen;
    [SerializeField]
    private  StartScreen _startScreen;
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private int _startLives;
    [SerializeField]
    private int _startWave;
    [SerializeField]
    private bool _showTutorial;

    private bool _isInWave = false;
    private Player _player;
    
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
        Spawn.CurrentWave = _startWave;
        _startScreen.gameObject.SetActive(true);
        _startScreen.OnStartGame += StartGame;
        
        _endScreen.OnEndGame += EndGame;
        Lives = _startLives;

    }

    void EndGame()
    {        
        _endScreen.gameObject.SetActive(false);
        _startScreen.gameObject.SetActive(true);
    }

    private void StartGame()
    {
        if (_showTutorial && Tutorial.NeedsTutorial)
        {
            GetComponent<Tutorial>().StartTutorial();
            GetComponent<Tutorial>().TutorialComplete += StartGame;
        }
        Spawn.CurrentWave = _startWave;

        Lives = _startLives;
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
        StartCoroutine(StartWave());
        Score = 0;
        InvokeRepeating("CheckEndWave", 1.0f, 1.0f);
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
        Camera.main.GetComponent<CameraShake>().ShakeCamera(0.1f, 0.03f);
        _player.Kill();
        Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.PlayerDead);
        if (Tutorial.SavePlayer)
        {
            Invoke("SpawnPlayer", 1.0f);
            return;
        }

        if (--Lives <= 0)
        {
            _isInWave = false;
            if (Score > PlayerPrefs.GetInt("Best"))
            {
                PlayerPrefs.SetInt("Best", Score);
            }
            Invoke("ShowEndScreen", 2);
            Spawn.StopSpawn();
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
        _player.ColorChanged += OnColorChanged;
        _player.PlayerKilled += PlayerKilled;
        ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Big, _player.transform.position, Color.white);
    }
     
    public Color GetRandomColor(Color color)
    {
        
        //int index = 0;
        //if (color == Color.white)
        //{
        //    return _colors[0];
        //}
        int count = Mathf.Min(Spawn.ColorCount, _colors.Length);

        //for (int i = 0; i < count; i++)
        //{
        //    if (color == _colors[i])
        //    {
        //        index = i + 1;
        //        break;
        //    }
        //}
        return _colors[/*index % count*/ Random.Range(0,count)];
    }

    private IEnumerator StartWave()
    {
        Spawn.CurrentWave++;
        Tooltip.Instance.ShowText("Wave " + Spawn.CurrentWave, Vector2.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        if (_player == null)
        {
            SpawnPlayer();
        }
        _isInWave = true;
        Spawn.SpawnWave(Spawn.CurrentWave);
    }
    private IEnumerator EndWave()
    {
        var energies = FindObjectsOfType<Energy>();
        foreach (var energy in energies)
        {
            Score += energy.Score;
            energy.Kill();
        }
        var enemies = FindObjectsOfType<Bacteria>();
        foreach (var enemy in enemies)
        {
            enemy.Kill();
        }
        Tooltip.Instance.ShowText("Success!", Vector2.zero, 1.0f);
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(StartWave());        
    }
    private void CheckEndWave()
    {
        if (Spawn.Enemies.Length == 0 && _isInWave)
        {
            _isInWave = false;
            StartCoroutine(EndWave());
        }
    }


}
