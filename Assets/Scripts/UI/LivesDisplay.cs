using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LivesDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _livePrefab;
    [SerializeField]
    private RectTransform _livesContent;

    private GameController _controller;
    private Player _player;
    void Start ()   
    {
        //FindObjectOfType<Player>().OnColorChanged += OnColorChanged;
        _controller = FindObjectOfType<GameController>();

	}

    private void OnPlayerKilled()
    {
        Invoke("UpdateLives", 0.1f);
    }
    private void UpdateLives()
    {
        for (int i = 0; i < _livesContent.childCount; i++)
        {
            Destroy(_livesContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < _controller.Lives; i++)
        {
            var go = Instantiate(_livePrefab, _livesContent) as GameObject;
            go.transform.localScale = Vector3.one;
        }
    }

    private void OnColorChanged(Color newColor)
    {
//        foreach (var item in GetComponentsInChildren<Graphic>())
//        {
//           // item.color = new Color(newColor.r, newColor.g, newColor.b, item.color.a);
//        }
    }

	void Update () 
    {
        if (_player == null)
        {
            _player = FindObjectOfType<Player>();
            if (_player != null)
            {
                UpdateLives();
                _player.OnColorChanged += OnColorChanged;
                _player.OnPlayerKilled += OnPlayerKilled;
            }
        }
	}
}