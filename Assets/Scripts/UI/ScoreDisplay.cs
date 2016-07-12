using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _valueDisplay;

    private GameController _controller;
    private Player _player;
    void Start ()   
    {
        //FindObjectOfType<Player>().OnColorChanged += OnColorChanged;
        _controller = FindObjectOfType<GameController>();
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
                _player.OnColorChanged += OnColorChanged;
            }
        }
        _valueDisplay.text = _controller.Score.ToString();
	}
}