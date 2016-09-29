using UnityEngine;
using System.Collections;

public class BacteriaMutate : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _mutateParticles;
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
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            _bacteria[i].Body.position = _bacteria.transform.position;
        }
        _bacteria.Color = _mutationColor;
        Emit();
    }

    private void Emit()
    {
        var exp = Instantiate(_mutateParticles);
        exp.startColor = _bacteria.Color;       
        exp.transform.position = transform.position;
        exp.Emit(500);
        exp.transform.SetParent(_bacteria.transform);
        Destroy(exp.gameObject, 5);
    }


    public void Clear()
    {
        if (IsMutating)
        {
            CancelInvoke("Mutate");
        }
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
            var bacteria = collision.gameObject.GetComponentInParent<Bacteria>();
            if (bacteria != null)
            {
                color = bacteria.Color;
            }
            else
            {
                return;
            }
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
