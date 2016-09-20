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

    public void Split(Bacteria bacteria, int startIndex, int endIndex)
    {
        for(int i = 0; i < bacteria.Vertices; i++)
        {
            Node node = bacteria[i];
            node.Disconnect();
            node.TargetBody = null;
            node.transform.parent = null;
            for (int j = 0; j < bacteria.Vertices; j++)
            {
                if (i != j)
                {
                    Physics2D.IgnoreCollision(node.Collider, bacteria[j].Collider, false);
                }
            }
        }

        List<Node> leftNodes = new List<Node>();
        List<Node> rightNodes = new List<Node>(bacteria.GetNodes());
        while (startIndex != endIndex)
        {
            var currentNode = bacteria[startIndex];
            leftNodes.Add(currentNode);
            startIndex = Indexer.GetIndex(Indexer.IndexType.After, startIndex, bacteria.Vertices);
        }
        rightNodes.RemoveAll(n => leftNodes.Contains(n));

        SpawnBacteriaFromNodes(leftNodes, bacteria.Radius * 0.5f);
        SpawnBacteriaFromNodes(rightNodes, bacteria.Radius * 0.5f);

        Destroy(bacteria.gameObject);

    }

    public void SpawnBacteriaFromNodes(List<Node> nodes, float radius)
    {
        Vector2 com = Vector2.zero;
        foreach (var node in nodes)
        {
            com += node.Body.position;
        }

        var left = ((GameObject)Instantiate(_bacteriaPrefab, com / nodes.Count, Quaternion.identity)).GetComponent<Bacteria>();
        left.SetNodes(nodes);
        left.Radius = radius;

    }

    public Bacteria SpawnBacteria(Vector3 position)
    {
        var newbac = Instantiate(_bacteriaPrefab, position, Quaternion.identity) as GameObject;
        return newbac.GetComponent<Bacteria>();
    }

    public Energy SpawnEnergy(Vector3 position, Vector3 initialDirection)
    {
        var obj = Instantiate(_energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.transform.localScale = Vector3.one * 0.12f;
        var random = Random.insideUnitCircle.normalized;
        energy.GetComponent<Rigidbody2D>().AddForce((Vector3.Dot(random, initialDirection) < 0 ? -random : random) * 4);
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