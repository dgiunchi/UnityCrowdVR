using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInputHandlerFromNavmesh : MonoBehaviour
{
    //private static List<HashSet<KeyCode>> Keys = new List<HashSet<KeyCode>>();
    private List<Dictionary<KeyCode, float>> Keys = new List<Dictionary<KeyCode, float>>();
    public AnimationConverterFromNavmesh navmeshGO;
    public Transform target;
    private int Capacity = 2;
    private int Clients = 0;
    public HashSet<KeyCode> allowed = new HashSet<KeyCode>();
    void OnEnable()
    {
        Clients += 1;
    }

    void OnDisable()
    {
        Clients -= 1;
    }

    public bool anyKey
    {
        get
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool isArrived()
    {
        return navmeshGO.isArrived();
    }

    void Update()
    {
        //@@TODO  HERE PUT THE VALUES FROM CONVRTER NAVMESH
        while (Keys.Count >= Capacity)
        {
            Keys.RemoveAt(0);
        }
        Dictionary<KeyCode, float> state = new Dictionary<KeyCode, float>();
        Vector3 navmeshposition = navmeshGO.transform.position;
        //Vector3 direction = navmeshGO.transform.forward;
        Vector3 direction = navmeshposition - transform.position;
        float distance = direction.magnitude;
       
        float navmeshTargetDist = (target.position - navmeshGO.transform.position).magnitude;
        float skeletonTargetDist = (target.position - transform.position).magnitude;

        float up = Vector3.Dot(Vector3.up, Vector3.Cross(transform.forward, direction));
         
        if(skeletonTargetDist < navmeshTargetDist)
        {
            distance = 0.0f;
            up = 0.0f;
        }
        //Debug.Log(value);
        //Debug.Log(direction);

        //allowed.Contains(k) //@@check if neeed
#if !UNITY_ANDROID || UNITY_EDITOR
        //direction
        
        //Debug.Log(up);
        if (up >= 0.0f ) //left
        {
            state[KeyCode.Q] = up;
        }
        else if( up < 0.0f ) // right
        {
            state[KeyCode.E] = up;
        }
        Keys.Add(state);

        //increment (actually no backward)
        if(distance >= 0)
        {
            state[KeyCode.W] = distance;
        }
        else if(distance < 0.0f) // right
        {
            state[KeyCode.S] = -distance;
        }
        Keys.Add(state);
        

#else 
        // @@TODO define KEY

        /*if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0)
        {
            state.Add(KeyCode.D);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < 0)
        {
            state.Add(KeyCode.A);
        }


        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y > 0)
        {
            state.Add(KeyCode.W);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y < 0)
        {
            state.Add(KeyCode.S);
        }

        if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x > 0) //|| OVRInput.Get(OVRInput.Button.Three)
        {
            state.Add(KeyCode.E);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x < 0) //|| OVRInput.Get(OVRInput.Button.Four)
        {
            state.Add(KeyCode.Q);
        }

         //Debug.Log("From  Debug:" + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x.ToString());

        Keys.Add(state);*/
#endif //!UNITY_ANDROID 
    }

    public float GetKey(KeyCode k)
    {
        if (Clients == 0)
        {
            return 0.0f;
        }
        else
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].ContainsKey(k))
                {
                    return Keys[i][k];
                }
            }
            return 0.0f; ;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }

}
