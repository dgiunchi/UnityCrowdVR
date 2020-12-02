using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{
    Transform target;
    Transform forward;
    Transform floor;
    GameObject ui;
    Bounds bounds;
    OVRPlayerController ovrpc;
    CharacterController cc;

    // Start is called before the first frame update
    void Awake()
    {
        
        target = GameObject.Find("OVRPlayerController").transform;
        forward = target.FindDeepChild("ForwardDirection").transform;
        floor = GameObject.Find("Ground").transform;      
        ovrpc = target.GetComponent<OVRPlayerController>();
        cc = target.GetComponent<CharacterController>();

        ovrpc.GravityModifier = 0;


        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        
        SetPlayerInitialPosition();
    }

    void SetPlayerInitialPosition()
    {

        target.position = new Vector3(bounds.center.x, floor.position.y + 1.8f, bounds.center.z);
        target.rotation = Quaternion.identity;

        //if (ui != null)
        //{
        //    ui.transform.position = forward.transform.position;
        //    ui.transform.rotation = forward.transform.rotation;
        //    ui.transform.Translate(forward.transform.forward * 2f);
        //}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target.position.y < 0)
        {
            ovrpc.GravityModifier = 0;
          
            SetPlayerInitialPosition();

        }
        else {

            ovrpc.GravityModifier = 1;
            
        }
    }

    //private void Update()
    //{
    //    if (ui == null)
    //    {
    //        getUi();
    //    }

        
    //}

    //void getUi() {
        
    //    if (ui == null) { 

    //        ui = GameObject.Find("EndOfTrialManager");
    //    }
        
    //    if (ui == null)
    //    {
    //        ui = GameObject.Find("Questionaire Panel");
    //    }

    //}
}
