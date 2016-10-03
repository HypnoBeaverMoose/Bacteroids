using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _valueDisplay;

    void Update () 
    {
        _valueDisplay.text = GameController.Instance.Score.ToString();
	}
}