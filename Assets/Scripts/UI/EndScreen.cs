using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour {


    public delegate void EndGame();
    public event EndGame OnEndGame;

    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Text _bestScoreText;

    private Vector3 _titleInitPos;
    private Vector3 _nextInitPos;

    // Use this for initialization
    void Start()
    {
        _titleInitPos = _titleText.rectTransform.position;
        _nextInitPos = _scoreText.rectTransform.position;
    }
    void OnEnable()
    {
        _scoreText.text = GameController.Instance.Score.ToString();
        _bestScoreText.text = PlayerPrefs.GetInt("Best").ToString();
        _bestScoreText.color = Color.white;
        if (_bestScoreText.text == _scoreText.text)
        {
            _bestScoreText.color = Color.red;
        }
    }
    // Update is called once per frame
    void Update () 
    {
        //_titleText.rectTransform.position = _titleInitPos + Mathf.Sin(Time.time) * Vector3.up * 0.2f;
        //_scoreText.rectTransform.position = _nextInitPos + Mathf.Sin(Time.time * 2) * Vector3.up * 0.1f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (OnEndGame != null)
            {
                OnEndGame();
            }
        }
    }
}
