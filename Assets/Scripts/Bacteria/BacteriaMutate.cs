﻿using UnityEngine;
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

    public void TriggerMutation()
    {
        _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
        Invoke("Mutate", _mutationTimeout);
    }

    public void TriggerMutation(float probability, Color color)
    {
        if (Random.value < probability)
        {
            _mutationColor = color;
            Invoke("Mutate", _mutationTimeout);
        }
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
            _mutationColor = GameController.Instance.GetRandomColor(_bacteria.Color);
            return;
        }
        for (int i = 0; i < _bacteria.Vertices; i++)
        {
            _bacteria[i].Body.position = _bacteria.transform.position;
        }
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
}
