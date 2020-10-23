using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimulationManager : MonoBehaviour
{
    public bool Inspect = false;

    public PositionSerializer serializer;
    private static SimulationManager instance = null;
    private Transform group;
    // Game Instance Singleton
    public static SimulationManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public GameObject skeletonRecordPrefab;
    public GameObject skeletonPlayPrefab;
    public List<GameObject> skeletons = new List<GameObject>();

    private float currentTime;
    private float currentTimeStep;

    public enum STATUS { RECORD, PLAY, PAUSE, STOP, NONE};
    public static STATUS status = STATUS.NONE;

    bool triggerPlay = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnStartRecord()
    {
        Time.captureDeltaTime = currentTimeStep;
        group = GameObject.Find("SkeletonsAnimatedFromSimulation").transform;

        int numberOfPersons = serializer.persons.Count;
        for (int i = 0; i < numberOfPersons; ++ i)
        {
            
            float initTime = serializer.personsOriginal[i].Keys.Min();
            Vector2 initialPosition = serializer.personsOriginal[i][initTime];
            GameObject obj = Instantiate(skeletonRecordPrefab, new Vector3(initialPosition.x, skeletonRecordPrefab.transform.position.y, initialPosition.y), Quaternion.identity); //@@TOODorientation??
            obj.transform.parent = group;
            obj.name = "Skeleton " + i.ToString();
            AnimationInputHandlerFromSimulation aihfs = obj.GetComponent<AnimationInputHandlerFromSimulation>();
            aihfs.timedPositions = new Dictionary<float, Vector2>(serializer.persons[i]);
            aihfs.SetInitalAndEningTimes();
            skeletons.Add(obj);

            if (initTime != serializer.initialTime) {
                obj.SetActive(false);
            }
        }

        serializer.UpdateSkeletonsToRecord(skeletons);
    }

    void OnStartPlay()
    {
        group = GameObject.Find("ToPlay").transform;

        int numberOfPersons = serializer.persons.Count;
        for (int i = 0; i < numberOfPersons; ++i)
        {

            float initTime = serializer.personsOriginal[i].Keys.Min();
            Vector2 initialPosition = serializer.personsOriginal[i][initTime];
            GameObject obj = Instantiate(skeletonPlayPrefab, new Vector3(initialPosition.x, skeletonPlayPrefab.transform.position.y, initialPosition.y), Quaternion.identity);//@@TOODorientation??
            obj.transform.parent = group;
            obj.name = "Skeleton " + i.ToString();
            skeletons.Add(obj);

            if (initTime != serializer.initialTime)
            {
                obj.SetActive(false);
            }
        }

        serializer.UpdateSkeletonsToRecord(skeletons);
    }

    public void Record()
    {

        status = STATUS.RECORD;
        serializer.Init();
        serializer.LoadFromCSV();
        serializer.CalculateInitialAndEndingTime();

        currentTimeStep = serializer.timeStep;
        currentTime = 0.0f;

        OnStartRecord();   
    }

    public void Play()
    {
        status = STATUS.PLAY;
        //this is a trick
        serializer.Init();
        serializer.LoadFromCSV();
        serializer.CalculateInitialAndEndingTime();

        currentTimeStep = serializer.timeStep;
        currentTime = 0.0f;

        OnStartPlay();        
    }

    public void Pause()
    {
        status = STATUS.PAUSE;
    }

    public void Stop()
    {
        status = STATUS.STOP;
    }

    public void Show()
    {
        foreach (GameObject go in skeletons)
        {
            go.SetActive(!go.activeSelf);
        }
    }

    public void ResetSimulation()
    {
        //@@todo implement
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        if (OVRInput.Get(OVRInput.Button.Three) && triggerPlay==false)
        {
            Play();
            triggerPlay = true;
        }
#endif



        if (status == STATUS.RECORD)
        {
            currentTime += currentTimeStep;
            currentTime = (float)System.Math.Round(currentTime, 3);
            AnimationInputHandlerFromSimulation.currentTime = currentTime;

            foreach (GameObject go in skeletons)
            {
                go.GetComponent<AnimationInputHandlerFromSimulation>().SetActiveByCurrenTime();
            }
        } else if (status == STATUS.PLAY)
        {
            // in serializer
        }
        else if (status == STATUS.PAUSE)
        {

        }
        else if (status == STATUS.STOP)
        {
            ResetSimulation();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SimulationManager))]
    public class SimulationManager_Editor : Editor
    {

        public SimulationManager Target;

        void Awake()
        {
            Target = (SimulationManager)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Target, Target.name);

            Inspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(Target);
            }
        }

        private void Inspector()
        {
            Utility.SetGUIColor(UltiDraw.Grey);
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                Utility.ResetGUIColor();

                if (Utility.GUIButton("Simulation-Animation", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Inspect = !Target.Inspect;
                }

                if (Target.Inspect)
                {
                    Target.skeletonRecordPrefab = EditorGUILayout.ObjectField("SkeletonRecord", Target.skeletonRecordPrefab, typeof(GameObject), true) as GameObject;
                    Target.skeletonPlayPrefab = EditorGUILayout.ObjectField("SkeletonPlay", Target.skeletonPlayPrefab, typeof(GameObject), true) as GameObject;

                    if (Utility.GUIButton("Record", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Record();
                    }

                    if (Utility.GUIButton("Play", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Play();
                    }
                    if (Utility.GUIButton("Pause", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Pause();
                    }
                    if (Utility.GUIButton("Stop", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Stop();
                    }
                    if (Utility.GUIButton("Show", UltiDraw.DarkGrey, UltiDraw.White))
                    {
                        Target.Show();
                    }

                }

            }
        }
    }
#endif
}
