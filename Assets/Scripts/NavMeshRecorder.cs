using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshRecorder : MonoBehaviour
{
    [HideInInspector]
    public int id = -1;
    public int gid = -1;
    [HideInInspector]
    public List<float[]> ToSerialize = new List<float[]>();
    public float currentTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Serialize(float currentTime)
    {

        //id,gid,x,y,dir_x,dir_y,radius,time
        float[] arr = new float[8];
        arr[0] = id;
        arr[1] = gid;
        arr[2] = transform.position.x;
        arr[3] = transform.position.z;
        arr[4] = transform.forward.x;
        arr[5] = transform.forward.z;
        arr[6] = 0.5f;
        arr[7] = currentTime;
        
        ToSerialize.Add(arr);
        
    }
}
