using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImmersiveCrowdExperimentStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
#if UNITY_EDITOR

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 30, 150, 30), "To the simulation..."))
        {
            // we can use here a staticclass for cross information activated by specific buttons in an interface
            SceneManager.LoadScene("sample_network_scene", LoadSceneMode.Single);
        }
    }

#endif
    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSimulation()
    {
        SceneManager.LoadScene("sample_network_scene", LoadSceneMode.Single);
    }

}
