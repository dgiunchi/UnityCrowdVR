using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationConverterFromSimulation : MonoBehaviour
{
    public List<List<Transform>> skeletonJoints;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        // read trajectories from a file or get the values from simulation
        // convert step by step trajectories in input for the network (need direction and movement)
        
        // feed the network through a script like InputHandler for each different skeleton
        // use PositionSerializer 
    }

    void Init()
    {
        GameObject[] sks = GameObject.FindGameObjectsWithTag("skeleton");
        List<GameObject> skeletons = new List<GameObject>(sks);
        skeletonJoints = new List<List<Transform>>();
        foreach (GameObject skeleton in skeletons)
        {
            Transform[] children = skeleton.transform.GetComponentsInChildren<Transform>();
            List<Transform> joints = new List<Transform>(children);
            skeletonJoints.Add(joints);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
