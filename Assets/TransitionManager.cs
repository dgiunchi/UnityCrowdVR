using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    private static TransitionManager instance = null;
    private Camera camera;
    private GameObject logo;


    private bool _isWaiting = false;
    public bool isWaiting
    {
        get { return _isWaiting; }
    }

    public static TransitionManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        camera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
    }

    public void setWaitingView()
    {
        _isWaiting = true;
        camera.backgroundColor = Color.black;
        camera.cullingMask = 1 << LayerMask.NameToLayer("logo");
        camera.clearFlags = CameraClearFlags.SolidColor;
    }

    public void setExperimentView ()
    {
        camera.cullingMask = ~(1 << LayerMask.NameToLayer("logo"));
        camera.clearFlags = CameraClearFlags.Skybox;
        _isWaiting = false;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
