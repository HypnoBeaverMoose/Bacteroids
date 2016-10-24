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

        _startScreen.gameObject.SetActive(true);
        if (Tutorial.NeedsTutorial)
        {
            _startScreen.OnStartGame += () =>
            {
                Invoke("SpawnPlayer", 1.0f);
                GetComponent<Tutorial>().StartTutorial();
            };
            GetComponent<Tutorial>().TutorialComplete += Spawn.StartSpawn;
        }
        else
        {
            _startScreen.OnStartGame += StartGame;
        }
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
        Invoke("SpawnPlayer", 1.0f);
        Score = 0;
        Spawn.StartSpawn();
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
        if (Tutorial.IsRunning)
        {
            Invoke("SpawnPlayer", 1.0f);
            return;
        }

        if (--Lives <= 0)
        {
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
}
