using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimulationNavMesh : MonoBehaviour
{
    List<GameObject> navMeshAgents = new List<GameObject>();

    class AgentProperties
    {
        public int id;
        public float initialTime;
        public float endingTime;
        public Vector2 initialPosition;
        public Vector2 endingPosition;
    };

    List<AgentProperties> infoAgents = new List<AgentProperties>();

    public UnityEngine.Object csvFile;
    public float scaleCsv = 1f;
    public float sceneHeight = 3f;
    public float scaleVelocity = 1f;

    public int precisionFloatLoad = 4;
    public GameObject navMeshPefab;
    public int framerate;
    private float seconds;
    [HideInInspector]
    public string path;
    private static SimulationNavMesh instance = null;
    public GameObject scenePrefab;
    private float initSimulationTime = -2.0f; //start at -1
    private bool serializedAll = false;

    private List<float[]> coordinates = new List<float[]>();
    bool isLoaded = false;
    bool allFinished = false;
    public static SimulationNavMesh Instance
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

    // Start is called before the first frame update
    void Start()
    {
        path = Application.streamingAssetsPath;
    }

    // Update is called once per frame
    void Update()
    {
        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.time;
        }
        
        float currenttime = (Time.time - initSimulationTime);

        allFinished = infoAgents.Count == 0 ? false : true; // check to avoi serialization before instantiating
        for (int i = 0; i < infoAgents.Count; ++i)
        {
            bool started = currenttime >= infoAgents[i].initialTime; //&& currenttime < infoAgents[i].endingTime // no sense the ending that comes from data
            bool arrived = navMeshAgents[i].GetComponent<AnimationConverterFromNavmesh>().HasTerminated();
            if(started == false || arrived == false)
            {
                allFinished = false;
            }
            if(started && !arrived)
            {
                navMeshAgents[i].GetComponent<NavMeshRecorder>().Serialize(currenttime);
            }
            
            navMeshAgents[i].SetActive(started && !arrived);
        }
        

        if(allFinished)
        {
            SerializeCSVAll();
        }
    }

    public Bounds getCsvTrajectoriesBounds()
    {

        GameObject points = new GameObject();
        points.name = "bounds";

        List<Vector3> newVertices = new List<Vector3>();

        int j = 0;

        foreach (var person in infoAgents)
        {
            var entryValue = person.initialPosition;
            newVertices.Add(new Vector3(entryValue.x, 0f, entryValue.y));
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

    public void ResizeScene()
    {


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

        Plane.transform.position = new Vector3(b.center.x, 0f, b.center.z);
        Plane.transform.localScale = new Vector3(widthX, 1f, widthY);

        Wall_1.transform.position = new Vector3(b.max.x, sceneHeight, b.center.z);
        Wall_1.transform.localScale = new Vector3(sceneHeight * 0.2f, 0.5f, widthY);

        Wall_2.transform.position = new Vector3(b.center.x, sceneHeight, b.min.z);
        Wall_2.transform.localScale = new Vector3(sceneHeight * 0.2f, 1f, widthX);

        Wall_3.transform.position = new Vector3(b.center.x, sceneHeight, b.max.z);
        Wall_3.transform.localScale = new Vector3(sceneHeight * 0.2f, 1f, widthX);

        Wall_4.transform.position = new Vector3(b.min.x, sceneHeight, b.center.z);
        Wall_4.transform.localScale = new Vector3(sceneHeight * 0.2f, 0.5f, widthY);


    }


    void Init()
    {
        if(isLoaded == false)
        {
            // load csv, // save initial position and ending position and times
            DeserializeFromCSV();
            isLoaded = true;
        }
    }

    void SerializeCSVAll()
    {
        if(serializedAll == false)
        {
            // merge all //@@todo
            int xprecision2 = 5;
            string formatString = "{0:G" + xprecision2 + "},{1:G" + xprecision2 + "},{2:G" + xprecision2 + "},{3:G" + xprecision2 + "},{4:G" + xprecision2 + "},{5:G" + xprecision2 + "},{6:G" + xprecision2 + "},{7:0.0000}";
            using (var outf = new StreamWriter(Path.Combine(path, "navMesh.csv")))
            {
                // serialize hader
                outf.WriteLine("id,gid,x,y,dir_x,dir_y,radius,time");
                for (int i = 0; i < navMeshAgents.Count; ++i)
                {
                    NavMeshRecorder rec = navMeshAgents[i].GetComponent<NavMeshRecorder>();
                    for(int j =0; j< rec.ToSerialize.Count; j++)
                    {
                        outf.WriteLine(formatString, rec.ToSerialize[j][0], rec.ToSerialize[j][1], rec.ToSerialize[j][2], rec.ToSerialize[j][3], rec.ToSerialize[j][4], rec.ToSerialize[j][5], rec.ToSerialize[j][6], rec.ToSerialize[j][7]);
                    }
                    
                }
            }
                
            serializedAll = true;
            Debug.Log("Serialization Ended");
        }
    }

    void DeserializeFromCSV()
    {
        CrowdCSVReader reader = new CrowdCSVReader();
        string fullPath = Path.Combine(path, csvFile.name + ".csv");
        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(path, "Assets", "StreamingAssets", csvFile.name + ".csv");
        }
        reader.Load(fullPath);
        List<CrowdCSVReader.Row> list = reader.GetRowList();

        int count = 0;
        int frameCount = 0;
        int numberOfColumns = 9;

        float[] arr = new float[numberOfColumns];
        int lastId = -1;
        foreach (CrowdCSVReader.Row row in list)
        {
            arr[0] = row.id != "" ? float.Parse(row.id) : float.MinValue;
            arr[1] = row.gid != "" ? float.Parse(row.gid) : float.MinValue;
            arr[2] = row.x != "" ? float.Parse(row.x) * scaleCsv : float.MinValue;
            arr[3] = row.y != "" ? float.Parse(row.y) * scaleCsv : float.MinValue;
            arr[4] = row.dir_x != "" ? float.Parse(row.dir_x) * scaleCsv : float.MinValue;
            arr[5] = row.dir_y != "" ? float.Parse(row.dir_y) * scaleCsv : float.MinValue;
            arr[6] = row.radius != "" ? float.Parse(row.radius) * scaleCsv : float.MinValue;
            arr[7] = row.time != "" ? (float)System.Math.Round(float.Parse(row.time), precisionFloatLoad) : float.MinValue;
            coordinates.Add(arr);
            count += numberOfColumns;
            frameCount += 1;


            if (lastId != arr[0])
            {
                AgentProperties prop = new AgentProperties();
                prop.id = (int)arr[0];
                prop.initialTime = arr[7];
                prop.initialPosition = new Vector2(arr[2], arr[3]);
                infoAgents.Add(prop);

                lastId = (int)arr[0];
            }

            infoAgents[infoAgents.Count - 1].endingTime = arr[7];
            infoAgents[infoAgents.Count - 1].endingPosition = new Vector2(arr[2], arr[3]);
            
        }
    }
    
    void InstantiateAllTheActors()
    {
        Transform group = GameObject.Find("NavmeshSimulationToRecord").transform;
        // us infoAgents and instantiate all the prefabs in the initial position. Set active to false.
        for (int i=0; i<infoAgents.Count; ++i)
        {
            float initTime = infoAgents[i].initialTime;
            Vector2 initialPosition = infoAgents[i].initialPosition;
            GameObject obj = Instantiate(navMeshPefab, new Vector3(initialPosition.x, navMeshPefab.transform.position.y, initialPosition.y), Quaternion.identity); //@@TOODorientation??
            obj.transform.parent = group;
            obj.name = "NavMeshAgent " + i.ToString();
            GameObject target = new GameObject("TargetFor " + obj.name );
            target.transform.parent = group;
            target.transform.position = new Vector3(infoAgents[i].endingPosition.x, navMeshPefab.transform.position.y, infoAgents[i].endingPosition.y);
            obj.GetComponent<AnimationConverterFromNavmesh>().target = target.transform;

            obj.GetComponent<NavMeshRecorder>().id = i;
            obj.GetComponent<NavMeshRecorder>().gid = i; // not important

            obj.SetActive(false);
            navMeshAgents.Add(obj);
        }

        allFinished = true; // start th check to see if it finished.
        initSimulationTime = -1.0f;
        Time.captureDeltaTime = 0.00139f * scaleVelocity;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SimulationNavMesh))]
    public class SimulationNavMesh_Editor : Editor
    {

        public SimulationNavMesh Target;

        void Awake()
        {
            Target = (SimulationNavMesh)target;
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
                Target.sceneHeight = EditorGUILayout.FloatField("Scale height:", Target.sceneHeight);
                Target.scenePrefab = EditorGUILayout.ObjectField("Scene prefab:", Target.scenePrefab, typeof(GameObject), true) as GameObject;
                Target.scaleVelocity = EditorGUILayout.FloatField("Capture Speed:", Target.scaleVelocity);
                /*if (Utility.GUIButton("Resize/Create Scene", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.ResizeScene();
                }*/

                ////////////////////////////////////////////

                Rect rect = EditorGUILayout.GetControlRect(false, 1f);
                rect.height = 1f;
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

                ////////////////////////////////////////////////////

                EditorGUILayout.LabelField("Simulation-NavMesh");

                Target.navMeshPefab = EditorGUILayout.ObjectField("NavMesh Prefab", Target.navMeshPefab, typeof(GameObject), true) as GameObject;
                
                if (Utility.GUIButton("Record", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Init();
                    Target.InstantiateAllTheActors();
                }
                if (Utility.GUIButton("Resize/Create Scene", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Init();
                    Target.ResizeScene();
                }
            }
        }
    }
#endif
}
