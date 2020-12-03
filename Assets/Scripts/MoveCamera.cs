using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public GameObject boundObject;
    private Bounds bounds;
    private Vector3 posObj;
    public float maxspeed=0.5f;

    public void Start()
    {
        if (boundObject != null)
        {
            Renderer[] renderers = boundObject.GetComponentsInChildren<Renderer>();
            bounds = new Bounds(transform.position, Vector3.zero);
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            bounds.extents = new Vector3(bounds.extents.x-0.4f, bounds.extents.y, bounds.extents.z - 0.4f);
        }

        posObj = gameObject.transform.position;

    }


    // Update is called once per frame
    void Update()
    {

        Vector2 pos = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        if (math.abs(pos.y) > maxspeed) pos.y = maxspeed * math.sign(pos.y);
        posObj += transform.forward * pos.y;
        if (bounds != null)
        {
            if (bounds.Contains(posObj)) 
            { 
                transform.position = posObj;
            }  
            else
            {
                posObj = transform.position;
            }
                
        }
        else {
            transform.position += transform.forward * -pos.y;
        }

        Vector3 euler = transform.rotation.eulerAngles;
        transform.Rotate(Vector3.up, pos.x);
    }

    
}
