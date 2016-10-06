using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private Sprite _gridElement;
    [SerializeField]
    private float _gridLineWitdh;
    [SerializeField]
    private int _verticalLines;
    [SerializeField]
    private float _alpha;
    [SerializeField]
    private Camera _camera;

    private int _horizonalLines;

    // Use this for initialization
    void Start ()
    {
        BuildGrid();
        _horizonalLines = (int)(_verticalLines / _camera.aspect);
    }

    public void BuildGrid()
    {
        float spacing = (2 * _camera.orthographicSize * _camera.aspect) / _verticalLines;
        for (int i = 0; i <= _verticalLines; i++)
        {
            AddGridLine(new Vector3(-_camera.orthographicSize * _camera.aspect + spacing * i, 0, 0), _camera.orthographicSize * 2, Quaternion.identity);
        }
        _horizonalLines = (int)(_verticalLines / _camera.aspect);
        spacing = (2 * _camera.orthographicSize ) / _horizonalLines;
        for (int i = 0; i <= _horizonalLines; i++)
        {
            AddGridLine(new Vector3(0, -_camera.orthographicSize + spacing * i, 0), _camera.orthographicSize * _camera.aspect * 2, Quaternion.AngleAxis(90,Vector3.forward));
        }

    }

    private void AddGridLine(Vector3 position, float length, Quaternion rotation)
    {
        var gridLine = new GameObject("GridLine");
        gridLine.transform.SetParent(transform);
        var rend = gridLine.AddComponent<SpriteRenderer>();
        rend.sprite = _gridElement;
        rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, _alpha);
        gridLine.transform.localScale = new Vector3(_gridLineWitdh, length, 1);
        gridLine.transform.localPosition = position;
        gridLine.transform.localRotation = rotation;
        gridLine.layer = gameObject.layer;
    }

}
