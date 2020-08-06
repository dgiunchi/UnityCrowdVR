using System;
using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
//using UnityEngine.Experimental.UIElements;
using Random = System.Random;
using Vector2 = RVO.Vector2;

public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;
    public Transform target;

    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    // Use this for initialization
    void Start()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, new Vector2(0.0f, 0.0f));

        // add in awake
        Simulator.Instance.processObstacles();

        //test
        CreatAgent(transform.position);
    }

    int GetAgentNumber(Vector3 position)
    {
        int agentNo = Simulator.Instance.queryNearAgent(new Vector2(position.x,position.z), 1.5f);
        return agentNo;
    }
    
    void DeleteAgent(int id)
    {
        if (id == -1 || !m_agentMap.ContainsKey(id))
            return;

        Simulator.Instance.delAgent(id);
        LeanPool.Despawn(m_agentMap[id].gameObject);
        m_agentMap.Remove(id);
    }

    void CreatAgent(Vector3 position)
    {
        int sid = Simulator.Instance.addAgent(new Vector2(position.x, position.z));
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, position, Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
            ga.target = target;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        Simulator.Instance.doStep();
    }
}