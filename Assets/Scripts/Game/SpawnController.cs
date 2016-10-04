using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnController : MonoBehaviour 
{

    public event System.Action OnBacteriaKilled;

    [SerializeField]
    private float _energyInitialForce;
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
        bacteria.Clear();
        bacteria.Vertices *= 2;
        startIndex *= 2;
        endIndex *= 2;
        for (int i = 0; i < bacteria.Vertices; i++)
        {
            Node node = bacteria[i];
            node.Disconnect();
            node.ClearEvents();
            node.TargetBody = null;
            node.transform.SetParent(null, true);
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
                
        SpawnBacteriaFromNodes(leftNodes, bacteria.Radius * 0.5f, bacteria.Color).GetComponent<BacteriaMutate>().TriggerMutation(0.5f);
        SpawnBacteriaFromNodes(rightNodes, bacteria.Radius * 0.5f, bacteria.Color).GetComponent<BacteriaMutate>().TriggerMutation(0.5f);
        Destroy(bacteria.gameObject);
    }

    public Bacteria SpawnBacteriaFromNodes(List<Node> nodes, float radius, Color color)
    {
        Vector2 com = Vector2.zero;
        foreach (var node in nodes)
        {
            com += node.Body.position;
            node.gameObject.SetActive(false);
        }

        var bacteria = ((GameObject)Instantiate(_bacteriaPrefab, com / nodes.Count, Quaternion.identity)).GetComponent<Bacteria>();
        bacteria.SetNodes(nodes);
        bacteria.Color = color;
        bacteria.Radius = radius;

        return bacteria;
    }

    public Bacteria SpawnBacteria(Vector3 position)
    {
        var newbac = Instantiate(_bacteriaPrefab, position, Quaternion.identity) as GameObject;
        return newbac.GetComponent<Bacteria>();
    }

    public Energy SpawnEnergy(Vector3 position, Vector3 initialDirection, float radius, Color color)
    {
        var obj = Instantiate(_energyPrefab, position, Quaternion.identity) as GameObject;
        var energy = obj.GetComponent<Energy>();
        energy.RadiusChange = radius;
        energy.Color = color;
        energy.GetComponent<Rigidbody2D>().AddForce(initialDirection * _energyInitialForce);
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