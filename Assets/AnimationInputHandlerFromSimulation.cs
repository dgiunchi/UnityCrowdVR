using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimationInputHandlerFromSimulation : MonoBehaviour
{
    //private static List<HashSet<KeyCode>> Keys = new List<HashSet<KeyCode>>();
    private List<Dictionary<KeyCode, float>> Keys = new List<Dictionary<KeyCode, float>>();
    private int Capacity = 2;
    private int Clients = 0;
    public HashSet<KeyCode> allowed = new HashSet<KeyCode>();

    public Dictionary<float, Vector2> timedPositions;// = new Dictionary<float, Vector2>();
    public static float currentTime = 0;

    public float initialTime;
    public float endingTime;


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

    public void SetInitalAndEningTimes()
    {
        initialTime = timedPositions.Keys.Min();
        endingTime = timedPositions.Keys.Max();
    }

    public bool isSimulating(float timeStamp)
    {
        //search in an array of hashes with timestamps for each person;
        return timeStamp >= initialTime && timeStamp <= endingTime;
    }

    public bool isArrived()
    {
        if(currentTime > endingTime)
            return true;
        return false;
    }

    public void SetActiveByCurrenTime()
    {
        gameObject.SetActive(isSimulating(currentTime));
    }

    Vector3 GetDirection()
    {
        //Debug.Log(currentTime);
        Vector2 value = timedPositions[currentTime];
        return new Vector3(value.x, 0.0f, value.y); 
    }

    void Update()
    {
        if (SimulationManager.status != SimulationManager.STATUS.RECORD) return;
        if (isArrived()) return;

        while (Keys.Count >= Capacity)
        {
            Keys.RemoveAt(0);
        }
        Dictionary<KeyCode, float> state = new Dictionary<KeyCode, float>();
       
        //Vector3 direction = navmeshGO.transform.forward;
        Vector3 direction = GetDirection();
        float distance = direction.magnitude;
        float up = Vector3.Dot(Vector3.up, Vector3.Cross(transform.forward, direction));
        
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
        

        /*// @@TODO define KEY

        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0)
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
