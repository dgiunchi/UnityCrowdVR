using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimationInputHandlerFromSimulation : MonoBehaviour
{

    public class TimedPosition
    {
        public float time;
        public Vector2 position;
        public Vector2 direction;

    };

    //private static List<HashSet<KeyCode>> Keys = new List<HashSet<KeyCode>>();
    private List<Dictionary<KeyCode, float>> Keys = new List<Dictionary<KeyCode, float>>();
    private int Capacity = 2;
    private int Clients = 0;
    public HashSet<KeyCode> allowed = new HashSet<KeyCode>();

    //public Dictionary<float, Vector2> timedPositions;// = new Dictionary<float, Vector2>();
    public List<TimedPosition> timedPositions;
    /*public float currentTime = 0;
    public float currentTimeStep = 0;*/
    public int currentIndex = 0;


    public float initialTime;
    public float endingTime;

    Vector2 currentDirection;

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
        initialTime = timedPositions[0].time;
        endingTime = timedPositions[timedPositions.Count-1].time;

        //old @@todo remove
        //initialTime = timedPositions.Keys.Min();
        //endingTime = timedPositions.Keys.Max();
    }

    public bool isSimulating(float timeStamp)
    {
        //search in an array of hashes with timestamps for each person;
        return timeStamp >= initialTime && timeStamp <= endingTime;
    }

    public bool isArrived()
    {
        //if(currentTime > endingTime)
        if (currentIndex >= timedPositions.Count)
            return true;
        return false;
    }

    public void SetActiveTime(float currentTime)
    {
        gameObject.SetActive(isSimulating(currentTime));
    }

    Vector3 GetDirection()
    {
        
        SIGGRAPH_2017.BioAnimation_Simulation component = GetComponent<SIGGRAPH_2017.BioAnimation_Simulation>();
        float threshold = 0.5f;
        float distance = component.distanceToTarget(timedPositions[currentIndex].position);
        bool arrived = distance < threshold;

        currentDirection = component.CalculateNewDirection(timedPositions[currentIndex].position);
        float dotProduct = Vector2.Dot(timedPositions[currentIndex].direction.normalized, currentDirection.normalized);

        if (arrived || dotProduct <=0.95)
    
        {
            component.Serialize();
            currentIndex++;        
            currentDirection = component.CalculateNewDirection(timedPositions[currentIndex].position);
            
        }

        Vector3 newCurrentdirection = new Vector3(currentDirection.x,0f,currentDirection.y);

        return newCurrentdirection; 
    }

    float sign;
    float angle;
    float distance;
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
        distance = direction.magnitude;
        angle = (float)Math.Abs((float)Math.Acos(Vector3.Dot(transform.forward, direction.normalized))) * 180f/(float)Math.PI;

#if !UNITY_ANDROID || UNITY_EDITOR

        if (float.IsNaN(angle)) return;

        sign =  Vector3.Cross(transform.forward, direction.normalized).y >= 0.0f ? 1 : -1;
        if (sign > 0) //left
        {
            state[KeyCode.Q] = angle;
        }
        else  // right
        {
            state[KeyCode.E] = -angle;
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (gameObject.name == "Skeleton 0")
        {
            for(int i=0; i <timedPositions.Count; ++i)
            {
                Vector2 where = timedPositions[i].position;
                Gizmos.DrawSphere( new Vector3(where.x, 0.1f, where.y),0.05f);
            }
            Gizmos.color = Color.green;
            Vector2 where2 = timedPositions[currentIndex].position;
            Gizmos.DrawSphere(new Vector3(where2.x, 0.1f, where2.y), 0.1f);
            Gizmos.DrawLine(new Vector3(transform.position.x, 1.0f, transform.position.z), new Vector3(where2.x, 1.0f, where2.y));

            //foward skeleton
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(transform.position.x, 1.0f, transform.position.z), new Vector3(transform.position.x, 1.0f, transform.position.z)+ transform.forward);

            // label
            GUI.color = Color.black;
            UnityEditor.Handles.color = Color.black;
            string stringa = "rotate:" + (sign * angle).ToString() + "\n distance:" + distance.ToString() ;
            UnityEditor.Handles.Label(new Vector3(transform.position.x, 1.0f, transform.position.z), stringa);

         
        }

        //Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }

}
