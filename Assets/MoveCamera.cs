using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setCameraParent(GameObject parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(parent.transform.position, Vector3.one);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Transform floor = GameObject.Find("Ground").transform;
        transform.position = new Vector3(bounds.center.x, floor.position.y + 2.0f, bounds.center.z);

        transform.parent = parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 pos = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector2 pos = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        transform.localPosition += transform.forward* pos.y;
        //Debug.Log(pos.ToString());
        
        Vector3 euler = transform.rotation.eulerAngles;
        transform.Rotate(Vector3.up, pos.x);
    }
}
