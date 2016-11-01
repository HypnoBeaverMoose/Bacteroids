using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LivesDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _livePrefab;
    [SerializeField]
    private RectTransform _livesContent;

    private void Awake()
    {
        transform.position = new Vector3(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize, 0);
        Player.PlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(Player obj)
    {
        obj.PlayerKilled += OnPlayerKilled;
        Invoke("UpdateLives", 1.0f);
    }

    private void OnPlayerKilled()
    {
        Invoke("UpdateLives", 0.1f);
    }

    private void UpdateLives()
    {
        for (int i = 0; i < _livesContent.childCount; i++)
        {
            Destroy(_livesContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < GameController.Instance.Lives; i++)
        {
            var go = Instantiate(_livePrefab, _livesContent) as GameObject;
            go.transform.localScale = Vector3.one;
        }
    }
}