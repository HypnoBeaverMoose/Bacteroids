using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _valueDisplay;

    private GameController _controller;
	void Start ()   
    {
        _controller = FindObjectOfType<GameController>();
	}
	
	
	void Update () 
    {
        _valueDisplay.text = _controller.Score.ToString();
	}
}
