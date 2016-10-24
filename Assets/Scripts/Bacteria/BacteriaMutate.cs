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
    public bool CanMutate { get { return _enableMutation; } set { _enableMutation = value; } }

    private Bacteria _bacteria;
    private Color _mutationColor;
    private bool _initialized = false;
    private Tutorial.HintEvent _hint;
    // Use this for initialization
    void Start ()
    {
	}

    public void TriggerMutation()
    {
        if (CanMutate)
        {
            _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
            Invoke("Mutate", _mutationTimeout);
        }
    }

    public void TriggerMutation(float probability, Color color)
    {
        if (CanMutate && Random.value < probability)
        {
            _hint = Tutorial.HintEvent.BacteriaMutatesBySplit;
            _mutationColor = color;
            Invoke("Mutate", _mutationTimeout);
        }
    }

    public void TriggerMutation(Color color)
    {
        if (CanMutate)
        {
            _mutationColor = color;
            Invoke("Mutate", _mutationTimeout);
        }
    }


    public void Mutate()
    {
        if (!CanMutate)
        {
            return;
        }
        if (!_initialized)
        {
            _mutateOnInit = true;
            _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
            return;
        }
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            _bacteria[i].Body.position = _bacteria.transform.position;
        }
        Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.BacteriaMutate, transform.position);
        Tutorial.Instance.ShowHintMessage(_hint, transform.position);
        _bacteria.Color = _mutationColor;
        ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Big, transform.position, _bacteria.Color);
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
            _hint = Tutorial.HintEvent.BacteriaMutatesByTouch;
            color = collision.gameObject.GetComponent<Player>().Color;
            randomMutation = true;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            _hint = Tutorial.HintEvent.BacteriaMutateByHit;
            color = collision.gameObject.GetComponent<Projectile>().Color;
            randomMutation = true;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            
            var bacteria = collision.gameObject.GetComponentInParent<Bacteria>();
            if (bacteria != null)
            {
                _hint = Tutorial.HintEvent.BacteriaMutatesByBacteria;
                color = bacteria.Color;
            }
            else
            {
                return;
            }
        }
        else if (collision.gameObject.CompareTag("Energy"))
        {
            _hint = Tutorial.HintEvent.BacteriaMutateByConsume;
            color = collision.gameObject.GetComponent<Energy>().Color;
        }

        if (CanMutate && !IsMutating && (_bacteria.Color == Color.white))
        {          
            if (randomMutation)
            {
                TriggerMutation();
            }
            else if (!randomMutation && color != _bacteria.Color)
            {
                TriggerMutation(color);
            }
        }
    }

    void OnDestroy()
    {
        if (_bacteria != null)
        {
            foreach (var node in _bacteria.GetNodes())
            {
                node.OnCollisionEnter -= NodeCollision;
            }
        }
    }
}
