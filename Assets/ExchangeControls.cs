using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExchangeControls : MonoBehaviour
{
    GameObject player;
    GameObject virtualPlayer;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("OVRPlayerController");
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<OVRPlayerController>().enabled = false;


        virtualPlayer = GameObject.Find("ImmersiveCamera");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<OVRPlayerController>().enabled = true;

        //back to the previous control
    }
}
