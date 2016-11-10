using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartScreen : MonoBehaviour {


    [SerializeField]
    private Text TitleText;
    [SerializeField]
    private Text StartText;


    private Vector3 titleInitPos;
    private Vector3 startInitPos;
    public delegate void StartGame();
    public event StartGame OnStartGame;

    void Start () 
    {
        titleInitPos = TitleText.rectTransform.position;
        startInitPos = StartText.rectTransform.position;
	}
	
	void Update () 
    {
        TitleText.rectTransform.position = titleInitPos + Mathf.Sin(Time.time) * Vector3.up * 0.05f;
        StartText.rectTransform.position = startInitPos + Mathf.Sin(Time.time * 2) * Vector3.up * 0.03f;
     
        if (Input.anyKey)
        {
            if (OnStartGame != null)
            {
                OnStartGame();
                gameObject.SetActive(false);
            }
        }
	}
}
