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
    public float MutateTimeoout { get { return _mutationTimeout; } set { _mutationTimeout = value; } }
    private Bacteria _bacteria;
    private Color _mutationColor;
    private bool _initialized = false;
    private Tutorial.HintEvent _hint = Tutorial.HintEvent.None;
    // Use this for initialization
    void Start ()
    {
	}

    public void TriggerMutation()
    {
        if (CanMutate)
        {
            _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
            if (_mutationColor != _bacteria.Color)
            {
                Invoke("Mutate", _mutationTimeout);
            }
        }
    }

    public void TriggerMutation(float probability, Color color)
    {
        if (CanMutate && Random.value < probability)
        {
            if (_hint == Tutorial.HintEvent.None)
            {
                Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.BacteriaMutatesBySplit, transform.position);
            }
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
            
            _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
            if (_mutationColor != _bacteria.Color)
            {
                _mutateOnInit = true;
            }
            return;
        }
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            _bacteria[i].Body.position = _bacteria.transform.position;
        }
        _bacteria.Color = _mutationColor;
        Tutorial.Instance.ShowHintMessage(Tutorial.HintEvent.BacteriaMutate, transform.position);
        ExplosionController.Instance.SpawnExplosion(ExplosionController.ExplosionType.Big, transform.position, _bacteria.Color);
        AudioController.Instance.PlaySound(SoundType.BacteriaMutate);
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
        Color color = _bacteria.Color;
        bool randomMutation = false;
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_hint == Tutorial.HintEvent.None)
            {
                _hint = Tutorial.HintEvent.BacteriaMutatesByTouch;
            }
            color = collision.gameObject.GetComponent<Player>().Color;
            randomMutation = true;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            if (_hint == Tutorial.HintEvent.None)
            {
                _hint = Tutorial.HintEvent.BacteriaMutateByHit;
            }
            color = collision.gameObject.GetComponent<Projectile>().Color;
            randomMutation = true;
        }
        //else if (collision.gameObject.CompareTag("Enemy"))
        //{
            
        //    var bacteria = collision.gameObject.GetComponentInParent<Bacteria>();
        //    if (bacteria != null)
        //    {
        //        if (_hint == Tutorial.HintEvent.None)
        //        {
        //            _hint = Tutorial.HintEvent.BacteriaMutatesByBacteria;
        //        }
        //        color = bacteria.Color;
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}
        else if (collision.gameObject.CompareTag("Energy"))
        {
            if (_hint == Tutorial.HintEvent.None)
            {
                _hint = Tutorial.HintEvent.BacteriaMutateByConsume;
            }
            color = collision.gameObject.GetComponent<Energy>().Color;
        }

        if (CanMutate && !IsMutating && (_bacteria.Color == Color.white))
        {
            if (randomMutation)
            {
                TriggerMutation();
                if (_hint != Tutorial.HintEvent.None && Tutorial.Instance.MessageShown(Tutorial.HintEvent.BacteriaMutate))
                {
                    Tutorial.Instance.ShowHintMessage(_hint, transform.position);
                    _hint = Tutorial.HintEvent.None;
                }
            }
            else if (!randomMutation && color != _bacteria.Color)
            {
                TriggerMutation(color);
                if (_hint != Tutorial.HintEvent.None && Tutorial.Instance.MessageShown(Tutorial.HintEvent.BacteriaMutate))
                {
                    Tutorial.Instance.ShowHintMessage(_hint, transform.position);
                    _hint = Tutorial.HintEvent.None;
                }
            }
            else
            {
                _hint = Tutorial.HintEvent.None;
            }
        }
        else
        {
            _hint = Tutorial.HintEvent.None;
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
