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
        int verticies = bacteria.Vertices;
        Destroy(bacteria.gameObject);
        yield return null;

        Bacteria newbac = SpawnBacteria(position, rotation, verticies - 1);
        yield return null;
        FindNearestNode(projectile.transform.position, projectile.transform.up, newbac).Body.AddForce(projectile.transform.up * 10, ForceMode2D.Impulse);
        SpawnEnergy(hit, -projectile.transform.up);
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

        SpawnEnergy(position, projectile.transform.up);
        SpawnEnergy(position, -projectile.transform.up);
        yield return null;

        var newbacteria = SpawnBacteria(position + normal * radius * 1.5f, rotation, verticies - 1);
        yield return null;
        FindNearestNode(position, normal, newbacteria).Body.AddForce(normal.normalized * 10, ForceMode2D.Impulse);

        newbacteria = SpawnBacteria(position - normal * radius * 1.5f, rotation, verticies - 1);
        yield return null;
        FindNearestNode(position, -normal, newbacteria).Body.AddForce(-normal.normalized * 10, ForceMode2D.Impulse);
    }

    private Bacteria SpawnBacteria(Vector3 position, Quaternion rotation,  int verticies)
    {
        var newbac = Instantiate(bacteriaPrefab, position, rotation) as GameObject;
        newbac.GetComponent<Bacteria>().Vertices = verticies;
        return newbac.GetComponent<Bacteria>();
    }

    private void SpawnEnergy(Vector3 position, Vector3 initialDirection)
    {
        var obj = Instantiate(energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.transform.localScale = Vector3.one * 0.12f;
        var random = Random.insideUnitCircle.normalized;
        energy.GetComponent<Rigidbody2D>().AddForce((Vector3.Dot(random, initialDirection) < 0 ? -random : random) * 5);
    }

    private void SpawnExplosion(Vector3 position)
    {
        var exp = Instantiate(pariclePrefab, position, Quaternion.identity) as GameObject;
        exp.GetComponent<ParticleSystem>().Emit(200);
        Destroy(exp, 5);
    }


    private Node FindNearestNode(Vector2 position, Vector2 direction, Bacteria bacteria)
    {
        var hit = Physics2D.Raycast(position, direction, Vector3.Distance(bacteria.transform.position, position) + bacteria.MaxSize, LayerMask.GetMask("Bacteria"));

        if (hit.collider != null)
        {
            Node node = hit.collider.GetComponent<Node>();
            if (bacteria.ContainsNode(node))
            {
                return node;
            }
        }
        return bacteria[Random.Range(0, bacteria.Vertices)];
    }
}
