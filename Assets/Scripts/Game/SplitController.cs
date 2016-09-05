using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitController : MonoBehaviour 
{

    public event System.Action OnBacteriaKilled;

    [SerializeField]
    private GameObject _bacteriaPrefab;
    [SerializeField]
    private GameObject _energyPrefab;
    [SerializeField]
    private GameObject _pariclePrefab;

 	void Awake ()
    {
 	}
 
    public void HandleHit(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        if (bacteria.Vertices <= Bacteria.MinVertexCount)
        {
            Destroy(bacteria.gameObject);
            for (int i = 0; i < bacteria.Vertices; i++)
            {
                var pos = bacteria[i].transform.position + (Vector3)Random.insideUnitCircle * bacteria.Radius;
                SpawnExplosion(pos);
                if (Random.value > 0.5f)
                {
                    SpawnEnergy(pos, Random.insideUnitCircle);
                }
            }
            if (OnBacteriaKilled != null)
            {
                OnBacteriaKilled();
            }
        }
        else if (Random.value > 0.5f)
        {
            StartCoroutine(HandleHitRoutine(bacteria, projectile, hit, velocity));
        }
        else
        {
            StartCoroutine(SplitCoroutine(bacteria, projectile, hit, velocity));
        }
    }

    private IEnumerator HandleHitRoutine(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        Vector3 position = bacteria.transform.position;
        Quaternion rotation = bacteria.transform.rotation;
        Vector3 direction = bacteria.GetComponent<BacteriaAI>().Direction;
        int verticies = bacteria.Vertices;
        Destroy(bacteria.gameObject);
        yield return null;

        Bacteria newbac = SpawnBacteria(position, rotation, verticies - 1);
        yield return null;
        FindNearestNode(projectile.transform.position, projectile.transform.up, newbac).Body.AddForce(projectile.transform.up * 10, ForceMode2D.Impulse);
        var energy = SpawnEnergy(hit, -projectile.transform.up);
        IgnoreCollision(energy.GetComponent<Collider2D>(), newbac);
        newbac.GetComponent<BacteriaAI>().Direction = direction;
    }

    private IEnumerator SplitCoroutine(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        Vector2 position = bacteria.transform.position;
        Quaternion rotation = bacteria.transform.rotation;
        Vector2 normal = new Vector2(projectile.transform.up.y, -projectile.transform.up.x);
        float radius = bacteria.Radius;
        int verticies = bacteria.Vertices;
        Destroy(bacteria.gameObject);
        SpawnExplosion(position);
        yield return null;

        var leftBacteira = SpawnBacteria(position + normal * radius * 1.5f, rotation, verticies - 1);
        yield return null;
        FindNearestNode(position, normal, leftBacteira).Body.AddForce(normal.normalized * 10, ForceMode2D.Impulse);


        var rightBacteira = SpawnBacteria(position - normal * radius * 1.5f, rotation, verticies - 1);
        yield return null;
        FindNearestNode(position, -normal, rightBacteira).Body.AddForce(-normal.normalized * 10, ForceMode2D.Impulse);

        var energy = SpawnEnergy(position, projectile.transform.up);
        IgnoreCollision(energy.GetComponent<Collider2D>(), leftBacteira);
        IgnoreCollision(energy.GetComponent<Collider2D>(), rightBacteira);

        energy = SpawnEnergy(position, -projectile.transform.up);
        IgnoreCollision(energy.GetComponent<Collider2D>(), leftBacteira);
        IgnoreCollision(energy.GetComponent<Collider2D>(), rightBacteira);

    }

    public Bacteria SpawnBacteria(Vector3 position, Quaternion rotation,  int verticies)
    {
        var newbac = Instantiate(_bacteriaPrefab, position, rotation) as GameObject;
        newbac.GetComponent<Bacteria>().Vertices = verticies;
        return newbac.GetComponent<Bacteria>();
    }

    public Energy SpawnEnergy(Vector3 position, Vector3 initialDirection)
    {
        var obj = Instantiate(_energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.transform.localScale = Vector3.one * 0.12f;
        var random = Random.insideUnitCircle.normalized;
        energy.GetComponent<Rigidbody2D>().AddForce((Vector3.Dot(random, initialDirection) < 0 ? -random : random) * 8);
        return energy;
    }

    private void SpawnExplosion(Vector3 position)
    {
        var exp = Instantiate(_pariclePrefab, position, Quaternion.identity) as GameObject;
        exp.GetComponent<ParticleSystem>().Emit(200);
        Destroy(exp, 5);
    }


    private void IgnoreCollision(Collider2D collider, Bacteria bacteria)
    {
        for (int i = 0; i < bacteria.Vertices; i++)
        {
            Physics2D.IgnoreCollision(collider, bacteria[i].Collider, true);
        }
    }

    private Node FindNearestNode(Vector2 position, Vector2 direction, Bacteria bacteria)
    {
        var hit = Physics2D.Raycast(position, direction, Vector3.Distance(bacteria.transform.position, position) + bacteria.Radius * 4, LayerMask.GetMask("Bacteria"));

        if (hit.collider != null)
        {
            Node node = hit.collider.GetComponent<Node>();
            if (bacteria.Contains(node))
            {
                return node;
            }
        }
        return bacteria[Random.Range(0, bacteria.Vertices)];
    }
}