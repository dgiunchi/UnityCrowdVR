using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInitialPosition : MonoBehaviour
{

    Transform target;
    Transform forward;
    Transform floor;
    Transform camera;
    GameObject ui;
    Bounds bounds;
    OVRPlayerController ovrpc;
    CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        
        target = GameObject.Find("OVRPlayerController").transform;
        forward = target.FindDeepChild("ForwardDirection").transform;
        camera = target.FindDeepChild("CenterEyeAnchor").transform;
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

        target.position = new Vector3(bounds.center.x, floor.position.y + 2.0f, bounds.center.z);
        target.rotation = Quaternion.identity;

        StartCoroutine(SetUiInitialPosition());

    }

    IEnumerator SetUiInitialPosition() {
        
        yield return new WaitForSeconds(0.5f);

        if (ui == null) ui = GameObject.Find("EndOfTrialManager");
        if (ui == null) ui = GameObject.Find("Questionaire Panel");

        if (ui != null)
        {

            ui.transform.position = new Vector3(bounds.center.x, camera.position.y, bounds.center.z);
            ui.transform.rotation = Quaternion.identity;
            ui.transform.Translate(ui.transform.forward * 2);
        }
        else {
            SetUiInitialPosition();
        }
    }


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

}
