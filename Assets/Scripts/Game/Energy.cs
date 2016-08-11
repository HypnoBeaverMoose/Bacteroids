﻿using UnityEngine;
using System.Collections;

public class Energy : MonoBehaviour 
{

    [SerializeField]
    private ParticleSystem _explosion;
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private float _spawnBacteriaTimeout;

    public float Amount { get; set; }
    public Color Color { get { return _renderer.color; } set { _renderer.color = value; } }
//    private Vector3 _originalScale;
//    private float _offset = 0;
	private void Start () 
    {
//        _originalScale = transform.localScale;
//        _offset = Random.value * 100;
        //Invoke("SpawnBacteria", _spawnBacteriaTimeout);
    }

    private void FixedUpdate()
    {
//        transform.localScale = _originalScale * 0.8f +
//            _originalScale * 0.2f * Mathf.Sin(Time.unscaledTime * 5 + _offset);              

    }

    private void Explode(int particleAmount)
    {
        var exp = Instantiate(_explosion);
        exp.startColor = Color;
        exp.transform.position = transform.position;
        exp.Emit(particleAmount);
        Destroy(exp.gameObject, 5);
        Destroy(gameObject);
    }

//    private void SpawnBacteria()
//    {
//        if (gameObject != null && Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Bacteria")) == null)
//        {            
//            FindObjectOfType<GameController>().SpawnBacteria(transform.position, 0.1f, 5, Color);
//            Explode(50);            
//            DestroyImmediate(gameObject);
//        }
//    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Explode(10);
        }
    }
}
