using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionFromLoading : MonoBehaviour
{
    private bool switchToGame = false;
    private bool isLoaded = false;
    public GameObject logo;

    private bool startLoad = false;
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SimulationManagerAdam.Instance.sceneLoaded)
        {
            //show the scene
            logo.SetActive(false);

            camera.cullingMask = ~(1 << LayerMask.NameToLayer("logo"));
            camera.clearFlags = CameraClearFlags.Skybox;
            
        } else //show the logo
        {
            logo.SetActive(true);

            camera.cullingMask = 1 << LayerMask.NameToLayer("logo");
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        if (startLoad == false)
        {
            LoadGame();
        }
        
    }


    void LoadGame()
    {
        startLoad = true;
        logo.GetComponent<TMPro.TextMeshPro>().text = SimulationManagerAdam.Instance.dataname + " Loading..."; 
        SimulationManagerAdam.Instance.LoadScene();
    }

}
