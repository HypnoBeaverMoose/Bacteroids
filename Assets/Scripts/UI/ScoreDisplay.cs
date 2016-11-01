using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _valueDisplay;
    void Awake()
    {
        transform.position = new Vector3( - Camera.main.orthographicSize * Camera.main.aspect, - Camera.main.orthographicSize, 0);
    }
    void Update () 
    {
        _valueDisplay.text = GameController.Instance.Score.ToString();
	}
}