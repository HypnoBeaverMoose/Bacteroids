using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tooltip : MonoBehaviour
{
    public const float DefaultDelay = 5.0f;

    public static Tooltip  Instance { get; private set;}
    
    public string Text { get { return _text; } }

    [SerializeField]
    private string _text;
    [SerializeField]
    private Text _textField;
    [SerializeField]
    private float _letterDelay;

    private GameObject _anchor = null;
    private Vector2 _offset = Vector2.zero;

    void Awake ()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void ShowText(string text, float delay = DefaultDelay)
    {
        _text = text;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(DrawText(delay));
    }

    public void ShowText(string text, Vector2 position, float delay = DefaultDelay)
    {
        transform.position = position;
        _text = text;
        StopAllCoroutines();
        StartCoroutine(DrawText(delay));
    }

    public void ShowText(string text, GameObject anchor, Vector2 offset, float delay = DefaultDelay)
    {
        _anchor = anchor;
        _offset = offset;
        _text = text;
        StopAllCoroutines();
       StartCoroutine(DrawText(delay));
    }

    public void Hide()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    private IEnumerator DrawText(float delay)
    {        
        _textField.text = "";
        
        string[] words = _text.Split(' ');
        foreach (var word in words)
        {
            float timer = _letterDelay;
            _textField.text += word + " ";
            yield return new WaitUntil(() => { return (timer -= Time.unscaledDeltaTime) < 0; });
        }
        if (delay > 0)
        {
            yield return new WaitUntil(() => { return (delay -= Time.unscaledDeltaTime) < 0; });
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_anchor != null)
        {
            transform.position = (Vector2)_anchor.transform.position + _offset;
        }
    }
}
