using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetPlayerInitialPosition();
    }

    void SetPlayerInitialPosition()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Bounds bounds = new Bounds(transform.position, Vector3.one);
        foreach (Collider c in colliders)
        {
            bounds.Encapsulate(c.bounds);
        }
        Transform target = GameObject.Find("OVRPlayerController").transform;
        target.position = new Vector3(bounds.center.x, bounds.center.y + 1.0f, bounds.center.z);
        Transform ui = GameObject.Find("EndOfTrialManager").transform;
        ui.position = new Vector3(target.position.x, target.position.y, target.position.z + 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
