using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationConverterFromNavmesh : MonoBehaviour 
{
    //this navmesh generates only Data for the simulation
    public Transform target;
    NavMeshAgent agent;

    private Vector3 increment = Vector3.zero;
    private Vector3 previousPosition;
    public float curSpeed;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        previousPosition = transform.localPosition;
        //Time.captureDeltaTime = 1.0f / 72.0f;
    }

    public bool isArrived()
    {
        Vector2 cur = new Vector2(transform.position.x, transform.position.z);
        Vector2 des = new Vector2(target.position.x, target.position.z);
        if ((cur - des).magnitude < 0.001f)
        {
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
        Vector3 curMove = transform.position - previousPosition;
        curSpeed = curMove.magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }

}
