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
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.one);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        Transform target = GameObject.Find("OVRPlayerController").transform;
        Transform floor =  transform.FindDeepChild("Ground");
        target.position = new Vector3(bounds.center.x, floor.position.y + 2.0f, bounds.center.z);
        Transform ui = GameObject.Find("EndOfTrialManager").transform;
        ui.position = new Vector3(target.position.x, target.position.y, target.position.z + 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
