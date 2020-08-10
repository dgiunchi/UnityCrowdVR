using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    BoxCollider area;
    public GameObject agentPefab;
    public GameObject target;
    public int numberOfAgents;

    private GameObject[] Agents;

    // Start is called before the first frame update
    void Start()
    {
        //init
        area = GetComponent<BoxCollider>();

        //create
        CreateBackgroundCrowd();
    }

    protected void CreateBackgroundCrowd()
    {
        Agents = new GameObject[numberOfAgents];
        // walkers reset
        Vector3 positionToInstantiate;
        Collider[] hitColliders;

        float minX = area.center.x - area.size.x / 2;
        float maxX = area.center.x + area.size.x / 2;

        float minZ = area.center.z - area.size.z / 2;
        float maxZ = area.center.z + area.size.z / 2;
        
        
        for (int i = 0; i < numberOfAgents; i++)
        {
            // attributes to be randomized
            float agentHeight = Random.Range(0.8f, 1.5f);
            float agentSize = Random.Range(0.8f, 1.2f);
            Agents[i] = GameObject.Instantiate(agentPefab);
            Agents[i].transform.localScale = new Vector3(agentSize, agentHeight, agentSize);
            // @@TODO ...continue randomization





            Agents[i].GetComponent<SampleAgentScript>().target = target.transform; 
            do
            {
                // extent of the hallway
                float xPosition = Random.Range(minX, maxX);
                float zPosition = Random.Range(minZ, maxZ);

                positionToInstantiate = new Vector3(xPosition, 0.5f, zPosition) + transform.position;
                hitColliders = Physics.OverlapSphere(positionToInstantiate, Agents[i].GetComponent<CapsuleCollider>().radius + 0.2f);
            } while (hitColliders.Length > 1);

            Agents[i].transform.position = positionToInstantiate;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
