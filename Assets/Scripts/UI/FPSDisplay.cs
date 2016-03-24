using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSDisplay : MonoBehaviour {

    [SerializeField]
    private Text _fpsDisplay;
    private float _fps = 60;
    public int SampleCount = 100;
    private int counter;
    private float accum = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        counter--;
        accum += Time.deltaTime;
        if(counter < 0)
        {
            counter = SampleCount;
            _fps = 1.0f / (accum / SampleCount);
            accum = 0;
            _fpsDisplay.text = ((int)_fps).ToString();
        }
        
	}
}
