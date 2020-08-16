using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Lean;
using RVO;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
//using UnityEngine.Experimental.UIElements;

using Vector2 = RVO.Vector2;

public class CrowdSpawner : SingletonBehaviour<CrowdSpawner>
{
    BoxCollider area;
    //public List<GameObject> agentPrefabList;
    public GameObject agentPrefab;
    public Transform target;

    public int numberOfAgents;

    private GameObject[] Agents;

    // for rvo2
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    void StartRVO2Simulator()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, new Vector2(0.0f, 0.0f));

        RVO2ObstacleCollection(GameObject.Find("ObstaclesRVO2").transform);

        // add in awake
        Simulator.Instance.processObstacles();
    }

    int GetRVO2AgentNumber(Vector3 position)
    {
        int agentNo = Simulator.Instance.queryNearAgent(new Vector2(position.x, position.z), 1.5f);
        return agentNo;
    }

    void DeleteRVO2Agent(int id)
    {
        if (id == -1 || !m_agentMap.ContainsKey(id))
            return;

        Simulator.Instance.delAgent(id);
        LeanPool.Despawn(m_agentMap[id].gameObject);
        m_agentMap.Remove(id);
    }

    void CreatRVO2Agent(Vector3 position, GameAgent agent)
    {
        int sid = Simulator.Instance.addAgent(new Vector2(position.x, position.z));
        if (sid >= 0)
        {
            agent.sid = sid;
            m_agentMap.Add(sid, agent);
            agent.target = target;
        }
    }

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
        if(agentPrefab.tag == "rvo2")
        {
            StartRVO2Simulator();
        }

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
            float agentHeight = UnityEngine.Random.Range(0.8f, 1.5f);
            float agentSize = UnityEngine.Random.Range(0.8f, 1.2f);

            
            Agents[i] = GameObject.Instantiate(agentPrefab);
            Agents[i].transform.localScale = new Vector3(agentSize, agentHeight, agentSize);
            // @@TODO ...continue randomization
            
            do
            {
                // extent of the hallway
                float xPosition = UnityEngine.Random.Range(minX, maxX);
                float zPosition = UnityEngine.Random.Range(minZ, maxZ);

                positionToInstantiate = new Vector3(xPosition, 0.5f, zPosition) + transform.position;
                hitColliders = Physics.OverlapSphere(positionToInstantiate, Agents[i].GetComponent<CapsuleCollider>().radius + 0.2f);
            } while (hitColliders.Length > 1);

            Agents[i].transform.position = positionToInstantiate;

            if (Agents[i].tag == "navmesh")
            {
                Agents[i].GetComponent<SampleAgentScript>().target = target;
            }
            else if(Agents[i].tag == "rvo2")
            {
                CreatRVO2Agent(positionToInstantiate, Agents[i].GetComponent<GameAgent>());
            }

        }
    }

    void RVO2ObstacleCollection(Transform parent)
    {
        BoxCollider[] boxColliders = parent.GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxColliders.Length; i++)
        {
            float minX = boxColliders[i].transform.position.x -
                         boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
            float minZ = boxColliders[i].transform.position.z -
                         boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;
            float maxX = boxColliders[i].transform.position.x +
                         boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
            float maxZ = boxColliders[i].transform.position.z +
                         boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;

            IList<Vector2> obstacle = new List<Vector2>();
            obstacle.Add(new Vector2(maxX, maxZ));
            obstacle.Add(new Vector2(minX, maxZ));
            obstacle.Add(new Vector2(minX, minZ));
            obstacle.Add(new Vector2(maxX, minZ));
            Simulator.Instance.addObstacle(obstacle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Simulator.Instance != null)
        {
            Simulator.Instance.doStep();
        }
    }
}
