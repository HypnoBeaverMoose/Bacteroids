using UnityEngine;
using System.Collections;

public class BacteriaMutate : MonoBehaviour
{
    [SerializeField]
    private bool _enableMutation;
    [SerializeField]
    private float _mutationTimeout;
    [SerializeField]
    private bool _mutateOnInit = false;

    public bool IsMutating { get { return IsInvoking("Mutate"); } }
    public bool CanMutate { get { return _enableMutation; } }

    private Bacteria _bacteria;
    private Color _mutationColor;
    private bool _initialized = false;

	// Use this for initialization
	void Start ()
    {
	}

    public void TriggerRandomMutation()
    {
        _mutationColor = GameController.Instance.GetRandomColor();
        Invoke("Mutate", _mutationTimeout);
    }

    public void TriggerMutation(Color color)
    {
        _mutationColor = color;
        Invoke("Mutate", _mutationTimeout);
    }


    public void Mutate()
    {
        if (!_initialized)
        {
            _mutateOnInit = true;
            _mutationColor = GameController.Instance.GetRandomColor();
            return;
        }
        _bacteria.Color = _mutationColor;

    }

    public void Init(Bacteria bacteria)
    {
        _bacteria = bacteria;
        foreach (var node in _bacteria.GetNodes())
        {
            node.OnCollisionEnter += NodeCollision;
        }
        _initialized = true;

        if (_mutateOnInit)
        {
            Mutate();
            _mutateOnInit = false;
        }

    }

    private void NodeCollision(Collision2D collision, Node node)
    {
        Color color = Color.black;
        bool randomMutation = false;
        if (collision.gameObject.CompareTag("Player"))
        {
            color = collision.gameObject.GetComponent<Player>().Color;
            randomMutation = true;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            color = collision.gameObject.GetComponent<Projectile>().Color;
            randomMutation = true;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            color = collision.gameObject.GetComponentInParent<Bacteria>().Color;
        }
        else if (collision.gameObject.CompareTag("Energy"))
        {
            color = collision.gameObject.GetComponent<Energy>().Color;
        }

        if (CanMutate && !IsMutating && (_bacteria.Color == Color.white))
        {
            if (randomMutation && color == _bacteria.Color)
            {
                TriggerRandomMutation();
            }
            else if(!randomMutation)
            {
                TriggerMutation(color);
            }
        }
    }
}
