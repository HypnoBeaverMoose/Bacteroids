using UnityEngine;
using System.Collections;

public class EventBox : MonoBehaviour
{
    public static Vector2 DefaultSize = Vector2.one * 100;

    public static EventBox Instance { get; private set; }

    // Use this for initialization
    void Awake ()
    {
        Instance = this;
        gameObject.SetActive(false);
	}

    public void Show(Vector2 center)
    {
        gameObject.SetActive(true);
        transform.position = center;
        StartCoroutine(ShowCoroutine(DefaultSize, Tooltip.DefaultDelay * 0.2f));
    }

    private IEnumerator ShowCoroutine(Vector2 size, float delay)
    {
        var rTransform = GetComponent<RectTransform>();
        rTransform.sizeDelta = Vector2.zero;
        Vector2 vel = Vector2.zero;
        while (rTransform.sizeDelta.magnitude < size.magnitude - 0.1f)
        {
            rTransform.sizeDelta = Vector2.SmoothDamp(rTransform.sizeDelta, size, ref vel, 0.1f);
            yield return null;
        }
        yield return new WaitUntil(() => { return (delay -= Time.unscaledDeltaTime) < 0; });
        gameObject.SetActive(false);
    }

}   
