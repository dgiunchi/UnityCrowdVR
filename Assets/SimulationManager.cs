using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Windows;
using System;
using System.Runtime.InteropServices;

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
    public GameObject rigidAvatarPrefab;
    public GameObject scenePrefab;
    public List<GameObject> skeletons = new List<GameObject>();
    public List<GameObject> rigidAvatars = new List<GameObject>();
    public UnityEngine.Object csvFile;
    public float scaleCsv =1f ;
    public float sceneHeight = 3f;

    public float currentTime;
    public float currentTimeStep;

    public enum STATUS { RECORD, PLAY, PLAYCSV, PAUSE, STOP, NONE};
    public static STATUS status = STATUS.NONE;

    bool triggerPlay = false;
    // Start is called before the first frame update
    void Start()
    {
        serializer.csvFileName = csvFile.name + ".csv";
        serializer.scaleValue = scaleCsv;
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
            Vector2 initialOrientation = serializer.persons[i][initTime];
            Vector3 newDirection = new Vector3(initialOrientation.x, 0.0f, initialOrientation.y);
            GameObject obj = Instantiate(skeletonRecordPrefab, new Vector3(initialPosition.x, skeletonRecordPrefab.transform.position.y, initialPosition.y), Quaternion.FromToRotation(transform.forward, newDirection)); //@@TOODorientation??
            obj.transform.parent = group;
            obj.name = "Skeleton " + i.ToString();
            AnimationInputHandlerFromSimulation aihfs = obj.GetComponent<AnimationInputHandlerFromSimulation>();
            aihfs.timedPositions = new List<AnimationInputHandlerFromSimulation.TimedPosition>(serializer.personsRecord[i]);
            aihfs.SetInitalAndEningTimes();
            skeletons.Add(obj);

            //if (initTime != serializer.initialTime) {
            //    obj.SetActive(false);
            //}
        }

        serializer.UpdateSkeletonsToRecord(skeletons);
    }

    public void OnStartPlay()
    {
        group = GameObject.Find("ToPlay").transform;

        

        int numberOfPersons = serializer.persons.Count;
        for (int i = 0; i < numberOfPersons; ++i)
        {

            float initTime = serializer.personsOriginal[i].Keys.Min();
            Vector2 initialOrientation = serializer.persons[i][initTime];
            Vector3 newDirection = new Vector3(initialOrientation.x, 0.0f, initialOrientation.y);
            Vector2 initialPosition = serializer.personsOriginal[i][initTime];
            GameObject obj = Instantiate(skeletonPlayPrefab, new Vector3(initialPosition.x, skeletonPlayPrefab.transform.position.y, initialPosition.y), Quaternion.FromToRotation(transform.forward, newDirection));//@@TOODorientation??
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

    void OnStartPlayCsv()
    {
        group = GameObject.Find("ToPlay").transform;

        rigidAvatars.Clear();

        int numberOfPersons = serializer.persons.Count;
        for (int i = 0; i < numberOfPersons; ++i)
        {

            float initTime = serializer.personsOriginal[i].Keys.Min();
            Vector2 initialPosition = serializer.personsOriginal[i][initTime];
            GameObject obj = Instantiate(rigidAvatarPrefab, new Vector3(initialPosition.x, rigidAvatarPrefab.transform.position.y, initialPosition.y), Quaternion.identity);//@@TOODorientation??
            obj.transform.parent = group;
            obj.name = "RigidAvatar " + i.ToString();
            rigidAvatars.Add(obj);

            if (initTime != serializer.initialTime)
            {
                obj.SetActive(false);
            }
        }

        serializer.UpdateRigidAvatars(rigidAvatars);
    }

    public void Record()
    {
        status = STATUS.RECORD;
        serializer.Init();
        serializer.LoadFromCSV();
        
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

#if UNITY_EDITOR
        
        currentTimeStep = serializer.timeStep;
        currentTime = 0.0f;
        OnStartPlay();
#endif

    }

    public void PlayCsv() {

        status = STATUS.PLAYCSV;
        //this is a trick
        serializer.Init();
        serializer.LoadFromCSV();
        serializer.CalculateInitialAndEndingTime();

        currentTimeStep = serializer.timeStep;
        currentTime = 0.0f;

        OnStartPlayCsv();

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

    public void Draw()
    {
        ///////////////////////////////////////////////////////

        GameObject trajectories = GameObject.Find("trajectories");

        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(trajectories);
        else
            UnityEngine.Object.Destroy(trajectories);

        ///////////////////////////////////////////////////


        serializer.csvFileName = csvFile.name + ".csv";
        serializer.scaleValue = scaleCsv;
        serializer.Init();
        serializer.LoadFromCSV();

        trajectories = new GameObject();
        trajectories.name = "trajectories";


        int j = 0;

        foreach (var person in serializer.personsOriginal)
        {

           

            GameObject myLine = new GameObject();

            float c = UnityEngine.Random.Range(0f, 1f);
            Color color = new Color(c, c, c);

            Vector2? start = null;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();

            Material yourMaterial = (Material)Resources.Load("Arrow", typeof(Material));

            lr.material = new Material(yourMaterial);
            lr.SetColors(color, color);
            lr.SetWidth(0.05f, 0.05f);

            lr.positionCount = person.Count;

            int i = 0;

            foreach (var entry in person)
            {
                var entryValue = (Vector2)entry.Value;

                if (start==null) {

                    start = (Vector2)entryValue;
                    myLine.transform.position = new Vector3(entryValue.x, 0, entryValue.y);
                } 
                             
                lr.SetPosition(i, new Vector3(entryValue.x, 0, entryValue.y));

                i++;
            }

            myLine.name = "person_" + j;
            myLine.transform.parent = trajectories.transform;
            j++;
        }


    }

    public void DrawBounds()
    {

        ///////////////////////////////////////////////////////
        
        GameObject boundLine =  GameObject.Find("boundsline");

        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(boundLine);
        else
            UnityEngine.Object.Destroy(boundLine);

        ///////////////////////////////////////////////////


        Bounds b = getCsvTrajectoriesBounds();

        var boundPoint1 = b.min;
        var boundPoint2 = b.max;
        var boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
        var boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
        var boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
        var boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
        var boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
        var boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);


        boundLine = new GameObject();

        float c = UnityEngine.Random.Range(0f, 1f);
        Color color = new Color(c, c, c);

        Vector2? start = null;
        boundLine.AddComponent<LineRenderer>();
        LineRenderer lr = boundLine.GetComponent<LineRenderer>();

        Material yourMaterial = (Material)Resources.Load("Arrow", typeof(Material));

        lr.material = new Material(yourMaterial);
        lr.SetColors(color, color);
        lr.SetWidth(0.05f, 0.05f);

        lr.positionCount = 5;

        int i = 0;

        boundLine.transform.position = boundPoint1;
        lr.SetPosition(0, boundPoint1);
        lr.SetPosition(1, boundPoint3);
        lr.SetPosition(2, boundPoint7);
        lr.SetPosition(3, boundPoint5);
        lr.SetPosition(4, boundPoint1);

        boundLine.name = "boundsline";



    }

    public void ResizeScene() {


        ///////////////////////////////////////////////////////

        GameObject scene = GameObject.Find("Scene");

        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(scene);
        else
            UnityEngine.Object.Destroy(scene);

        ///////////////////////////////////////////////////

        scene = Instantiate(scenePrefab);
        scene.name = "Scene";

        GameObject Wall_1 = GameObject.Find("Wall_1");
        GameObject Wall_2 = GameObject.Find("Wall_2");
        GameObject Wall_3 = GameObject.Find("Wall_3");
        GameObject Wall_4 = GameObject.Find("Wall_4");
        GameObject Plane = GameObject.Find("Plane");

        /////////////////////////////////////////////////
        
        Bounds b = getCsvTrajectoriesBounds();

        float widthX = (b.extents.x * 2) / 10;
        float widthY = (b.extents.z * 2) / 10;

        Plane.transform.position = new Vector3(b.center.x, 0f,b.center.z);
        Plane.transform.localScale = new Vector3(widthX, 1f, widthY);

        Wall_1.transform.position = new Vector3(b.max.x, sceneHeight, b.center.z);
        Wall_1.transform.localScale = new Vector3(sceneHeight * 0.2f, 0.5f, widthY);

        Wall_2.transform.position = new Vector3(b.center.x, sceneHeight, b.min.z);
        Wall_2.transform.localScale = new Vector3(sceneHeight * 0.2f, 1f, widthX);

        Wall_3.transform.position = new Vector3(b.center.x, sceneHeight, b.max.z);
        Wall_3.transform.localScale = new Vector3(sceneHeight*0.2f, 1f, widthX);

        Wall_4.transform.position = new Vector3(b.min.x, sceneHeight, b.center.z);
        Wall_4.transform.localScale = new Vector3(sceneHeight * 0.2f, 0.5f, widthY);


    }

    public Bounds getCsvTrajectoriesBounds() {

        serializer.csvFileName = csvFile.name + ".csv";
        serializer.scaleValue = scaleCsv;
        serializer.Init();
        serializer.LoadFromCSV();

        GameObject points = new GameObject();
        points.name = "bounds";

        List<Vector3> newVertices = new List<Vector3>();

        int j = 0;

        foreach (var person in serializer.personsOriginal)
        {

            foreach (var entry in person)
            {
                var entryValue = (Vector2)entry.Value;

                newVertices.Add(new Vector3(entryValue.x, 0f, entryValue.y));

            }


        }

        newVertices.Add(new Vector3(0f, sceneHeight, 0f)); ///this is to give some hight 


        MeshFilter meshfilter = points.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshfilter.mesh = mesh;
        mesh.vertices = newVertices.ToArray();
        Bounds b = mesh.bounds;

        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(points);
        else
            UnityEngine.Object.Destroy(points);

        return b;
    }

    public float initTime = 0;
    // Update is called once per frame
    void Update()
    {

#if UNITY_ANDROID
        if (OVRInput.Get(OVRInput.Button.Three) && triggerPlay==false)
        {
            triggerPlay = true;
            Play();
        }
#endif



        if (status == STATUS.RECORD)
        {
            if(initTime == 0)
            {
                initTime = Time.fixedTime;
            }
            currentTime = Time.fixedTime - initTime;
            currentTime = (float)System.Math.Round(currentTime, 2);

            /*foreach (GameObject go in skeletons)
            {
                 go.GetComponent<AnimationInputHandlerFromSimulation>().SetActiveByCurrenTime();
            }*/

        }
        else if (status == STATUS.PLAYCSV)
        {
            // in serializer
        }
        else if (status == STATUS.PLAY)
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

                EditorGUILayout.LabelField("Data import");

                Target.csvFile = EditorGUILayout.ObjectField("Csv Data file to load:", Target.csvFile, typeof(UnityEngine.Object), true) as UnityEngine.Object;
                Target.scaleCsv = EditorGUILayout.FloatField("Scale CSV by:", Target.scaleCsv);
                Target.sceneHeight = EditorGUILayout.FloatField("Scale CSV by:", Target.sceneHeight);
                Target.scenePrefab = EditorGUILayout.ObjectField("Scene prefab:", Target.scenePrefab, typeof(GameObject), true) as GameObject;

                if (Utility.GUIButton("Draw Trajectories", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Draw();
                }
                if (Utility.GUIButton("Draw Boundaries", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.DrawBounds();
                }
                if (Utility.GUIButton("Resize/Create Scene", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.ResizeScene();
                }

                ////////////////////////////////////////////

                Rect rect = EditorGUILayout.GetControlRect(false, 1f);
                rect.height = 1f;
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

                ////////////////////////////////////////////////////
                
                EditorGUILayout.LabelField("Simulation-Animation");

                Target.skeletonRecordPrefab = EditorGUILayout.ObjectField("SkeletonRecord", Target.skeletonRecordPrefab, typeof(GameObject), true) as GameObject;
                Target.skeletonPlayPrefab = EditorGUILayout.ObjectField("SkeletonPlay", Target.skeletonPlayPrefab, typeof(GameObject), true) as GameObject;
                Target.rigidAvatarPrefab = EditorGUILayout.ObjectField("RigidAvatarPlayCsv", Target.rigidAvatarPrefab, typeof(GameObject), true) as GameObject;             

                if (Utility.GUIButton("Record", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Record();
                }

                if (Utility.GUIButton("Play", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Play();
                }
                if (Utility.GUIButton("PlayCSV", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.PlayCsv();
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
#endif
}
