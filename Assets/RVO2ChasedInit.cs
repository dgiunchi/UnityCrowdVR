using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RVO2ChasedInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ActivateRVO2();
    }

    void ActivateRVO2()
    {
        GameObject.Find("InitCrowdArea").GetComponent<CrowdSpawner>().CreatRVO2Agent(transform.position, GetComponent<GameAgent>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
