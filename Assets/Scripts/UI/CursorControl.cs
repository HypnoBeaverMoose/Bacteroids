using UnityEngine;
using System.Collections;

public class CursorControl : MonoBehaviour {

    public static Vector2 Position;



    [SerializeField]
    private float _zOffset;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _resposiveness;
    [SerializeField]
    private bool _stickToPlayer;


    private Player _player;
    private Vector3 _lastPos;
    private Vector3 _position;
    private Vector3 _offset;
    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        Player.OnPlayerSpawned += PlayerSpawned;
        _lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void PlayerSpawned(Player player)
    {
        _player = player;
    }

	// Update is called once per frame
	void Update ()
    {
        
        
        if (_stickToPlayer && _player != null)
        {
            _position = _player.transform.position + _offset;
            _position.z = _player.transform.position.z;
            _offset -= (_lastPos - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * _resposiveness;
            _offset = Vector3.ClampMagnitude(_offset, _radius);
            _lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        }
        transform.position = Vector3.Lerp(transform.position, new Vector3(_position.x, _position.y, _zOffset),1);
        Position = transform.position;

    }
}
