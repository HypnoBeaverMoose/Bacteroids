using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class HighScoreScreen : MonoBehaviour 
{
    public delegate void SkipScores();
    public event SkipScores OnSkipScores;

    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private Text _highScoreField;
	// Use this for initialization
	void OnEnable () 
    {
        _highScoreField.text = "";
        _highScoreField.transform.localPosition = new Vector3(_highScoreField.transform.localPosition.x, -65, _highScoreField.transform.localPosition.z);
        for (int i = 0; i < HighScores.ScoreCount(); i++)
        {
            string name = "";
            int score = 0;
            HighScores.GetScore(i, out name, out score);
            _highScoreField.text += (i + 1).ToString() + "." + name + " " + score.ToString() + "\n";
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        _highScoreField.transform.Translate(0, Time.deltaTime * _moveSpeed, 0);
        if (_highScoreField.transform.localPosition.y > 1000 || (Input.GetKeyDown(KeyCode.Space)))
        {
            if (OnSkipScores != null)
            {
                OnSkipScores();
            }
        }
	}
}
