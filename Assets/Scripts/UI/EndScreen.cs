using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour {


    public delegate void EndGame();
    public event EndGame OnEndGame;

    [SerializeField]
    private Text _titleText;
    [SerializeField]
    private Text _nextText;
    [SerializeField]
    private InputField _nameInput;

    private Vector3 _titleInitPos;
    private Vector3 _nextInitPos;

	// Use this for initialization
	void Start ()
    {
        _titleInitPos = _titleText.rectTransform.position;
        _nextInitPos = _nextText.rectTransform.position;
	}

    void OnEnable()
    {
        _nameInput.text = "___";
    }
	

	// Update is called once per frame
	void Update () 
    {
        _titleText.rectTransform.position = _titleInitPos + Mathf.Sin(Time.time) * Vector3.up * 0.2f;
        _nextText.rectTransform.position = _nextInitPos + Mathf.Sin(Time.time * 2) * Vector3.up * 0.1f;

        string name = _nameInput.text;
        name = name.PadRight(3, '_');
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '_')
            {
                _nameInput.caretPosition = Mathf.Min(_nameInput.caretPosition, i);
            }
        }
        name = name.Substring(0, 3);
        name = name.ToUpper();
        _nameInput.text = name;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (OnEndGame != null)
            {
                OnEndGame();
            }
        }
    }
}
