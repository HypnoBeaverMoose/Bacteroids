using UnityEngine;
using System.Collections;

public class SplitController : MonoBehaviour 
{
    private static SplitController _instance = null;
	
    [SerializeField]
    private GameObject bacteriaPrefab;
    [SerializeField]
    private GameObject energyPrefab;
    [SerializeField]
    private GameObject pariclePrefab;


	void Awake () 
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
	}
	
    public static void HandleHit(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        if (_instance != null)
        {
            _instance.HandleBacteriaHit(bacteria, projectile, hit, velocity);
        }
    }

    private void HandleBacteriaHit(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        StartCoroutine(SplitCoroutine(bacteria, projectile, hit, velocity));
        return;
        if (bacteria.Vertices <= Bacteria.MinVertexCount)
        {
            Destroy(bacteria.gameObject);    
            for (int i = 0; i < bacteria.Vertices; i++)
            {
                var pos = bacteria[i].transform.position + (Vector3)Random.insideUnitCircle * bacteria.Radius;
                SpawnExplosion(pos);
                if (Random.value > 0.5f)
                {
                    SpawnEnergy(pos);
                }
            }
        }
        else
        {
            StartCoroutine(HandleHitRoutine(bacteria, projectile, hit, velocity));
        }
    }

    private void SpawnEnergy(Vector3 position)
    {
        var obj = Instantiate(energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.transform.localScale = Vector3.one * 0.12f;
        energy.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle * 3);
    }

    private void SpawnExplosion(Vector3 position)
    {
        var exp = Instantiate(pariclePrefab, position, Quaternion.identity) as GameObject;
        exp.GetComponent<ParticleSystem>().Emit(200);
        Destroy(exp, 5);
    }

    private IEnumerator HandleHitRoutine(Bacteria bacteria, Projectile projectile, Vector2 hit, Vector2 velocity)
    {
        Bacteria newbac = SpawnBacteria(bacteria.transform.position, bacteria.transform.rotation, bacteria.Vertices - 1, bacteria.Radius);
        Destroy(bacteria.gameObject);

        yield return null;
        var cast = Physics2D.Raycast(hit, ((Vector2)newbac.transform.position - hit).normalized);

        SpawnEnergy(hit);
        if (cast.collider != null)
        {
            var node = cast.collider.GetComponent<Node>();
            if (node != null)
            {
                node.Body.AddForce(((Vector2)newbac.transform.position - hit).normalized * 10, ForceMode2D.Impulse); 
            }
        }
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
        var bac = SpawnBacteria(position + normal * radius * 1.5f, rotation, verticies - 1, radius);
        yield return null;
        var  raycastHit = Physics2D.Raycast(position, normal.normalized);
        if (raycastHit.collider != null && raycastHit.collider.GetComponent<Node>() != null)
        {
            raycastHit.collider.GetComponent<Node>().Body.AddForce(normal.normalized * 10, ForceMode2D.Impulse);
        }
        SpawnBacteria(position - normal * radius * 1.5f, rotation, verticies - 1, radius);
        yield return null;
        raycastHit = Physics2D.Raycast(position, -normal.normalized);
        if (raycastHit.collider != null && raycastHit.collider.GetComponent<Node>() != null)
        {
            raycastHit.collider.GetComponent<Node>().Body.AddForce(-normal.normalized * 10, ForceMode2D.Impulse);
        }
    }

    private Bacteria SpawnBacteria(Vector3 position, Quaternion rotation,  int verticies, float radius)
    {
        var newbac = Instantiate(bacteriaPrefab, position, rotation) as GameObject;
        newbac.GetComponent<Bacteria>().Vertices = verticies - 1;
        newbac.GetComponent<Bacteria>().Radius = radius;
        return newbac.GetComponent<Bacteria>();
    }
}
