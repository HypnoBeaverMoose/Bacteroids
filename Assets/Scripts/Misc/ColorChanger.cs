using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    [SerializeField]
    private float _interval;
    void Awake()
    {
        Player.OnPlayerSpawned += PlayerSpawned;
    }

    private void PlayerSpawned(Player player)
    {
        player.OnColorChanged += OnColorChanged;
    }

    private void OnColorChanged(Color newColor)
    {
        StopAllCoroutines();
        StartCoroutine(ColorChangeRoutine(newColor));
    }

    private IEnumerator ColorChangeRoutine(Color newColor)
    {
        foreach (var graphic in GetComponentsInChildren<Graphic>())
        {
            graphic.color = new Color(newColor.r, newColor.g, newColor.b, graphic.color.a);
            yield return new WaitForSeconds(_interval);
        }

        foreach (var graphic in GetComponentsInChildren<SpriteRenderer>())
        {
            graphic.color = new Color(newColor.r, newColor.g, newColor.b, graphic.color.a);
            yield return new WaitForSeconds(_interval);
        }
    }
}
