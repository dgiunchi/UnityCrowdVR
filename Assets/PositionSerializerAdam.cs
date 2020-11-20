using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System;

public class PositionSerializerAdam : MonoBehaviour
{

    //public string datafile;

    public List<List<Transform>> skeletonJoints;

    public List<Transform> rigidAvatars;
    List<GameObject> skeletonsList;


    private string csvFileName;
    private string datafile;
    private string txtFileName;
    public string Name {
        set {
            csvFileName = value + ".csv";
            datafile = value + ".dat";
            txtFileName = value + ".txt";
        } 
    }


    static public Dictionary<string, string> d = new Dictionary<string, string>();

    private int count = 0;
    private int countPlay = 0;
    public float scaleValue = 1f;

    private float seconds;
    [HideInInspector]
    public static int framerate = 72;
    private int skeletonNumbers = 1; //change to the real number or move code to Start.
    public static int jointsNumbers;
    public static int timeAndIndex = 0;
    public static int positionCoord = 4;
    
    private float[] coordinates;
    
    public static int numberOfValuesPerFrame;

    string path;
    float currentTime = 0;
    float initSimulationTime = -1.0f;
    //string path = @"C:\Users\dannox\Desktop\crowdCount\CrowdVR\UnityCrowdVR\";
    
    bool serializationDone = false;

    private int allFramesAndPersonsFromCSVLoad = 0; //number of lines of the cvd
    private int dataPerPersonAndFrameFromCSVLoad = 0; // number of columns
    private int numberOfPesonsFromCSVLoad = 0; // total number of persons    
    private float[] csvCoordinates;
    List<int> csvNumberOfFramesPerPerson;
    private float[] csvVariationsCoordinates;
    public List<Dictionary<float, int>> timeFrameToIndex = new List<Dictionary<float, int>>();
    private List<int> peopleIndexes = new List<int>();
    public List<Dictionary<float, Vector2>> personsOriginal = new List<Dictionary<float, Vector2>>();
    public List<Dictionary<float, Vector2>> persons = new List<Dictionary<float, Vector2>>();
    public List<List<AnimationInputHandlerFromAdamSimulation.TimedPosition>> personsRecord = new List<List<AnimationInputHandlerFromAdamSimulation.TimedPosition>>();
    public float timeStep;

    private float initialTime = float.MaxValue;
    private float endingTime = float.MinValue;
    private float simulationTimeLength;
    public static bool forceSerialization = false;
    public int precisionFloatLoad = 3;
    private int lastCountPlay = -1;

    private int n1, n2, n3;
    int frameStartIndex;
    int[] v = new int[9];

    TextAsset csvAsset;

    WWW file;
    [HideInInspector]
    bool csvLoaded = false;
    bool csvDidLoad = false;

    WWW www;
    bool binaryLoaded = false;
    bool binaryDidLoad = false;

    private void Awake()
    {
        d["Skeleton"] = "Bip01";

        d["Hips"] = "Bip01_Pelvis";
        d["Chest"] = "Bip01_Spine";
        d["Chest2"] = "Bip01_Spine1";
        d["Chest3"] = "Bip01_Spine2";
        d["Chest4"] = "";

        d["Neck"] = "Bip01_Neck";
        d["Head"] = "Bip01_Head";
        d["HeadSite"] = "Bip01_HeadNub";

        d["RightCollar"] = "";
        d["RightShoulder"] = "Bip01_L_UpperArm";
        d["RightElbow"] = "Bip01_L_Forearm";
        d["RightWrist"] = "Bip01_L_Hand";
        d["RightWristSite"] = "Bip01_L_Finger0";

        d["LeftCollar"] = "";
        d["LeftShoulder"] = "Bip01_R_UpperArm";
        d["LeftElbow"] = "Bip01_R_Forearm";
        d["LeftWrist"] = "Bip01_R_Hand";
        d["LeftWristSite"] = "Bip01_R_Finger0";

        d["RightHip"] = "Bip01_L_Thigh";
        d["RightKnee"] = "Bip01_L_Calf";
        d["RightAnkle"] = "Bip01_L_Foot";
        d["RightToe"] = "Bip01_L_Toe0";
        d["RightToeSite"] = "Bip01_L_Toe0Nub";

        d["LeftHip"] = "Bip01_R_Thigh";
        d["LeftKnee"] = "Bip01_R_Calf";
        d["LeftAnkle"] = "Bip01_R_Foot";
        d["LeftToe"] = "Bip01_R_Toe0";
        d["LeftToeSite"] = "Bip01_R_Toe0Nub";
    }
    void Start()
    {
        jointsNumbers = getJointsNumber(SimulationManagerAdam.Instance.skeletonRecordPrefab);
    }
    public void Init()
    {
        path = Application.streamingAssetsPath;

        file = null;
        csvLoaded = false;
        csvDidLoad = false;

        www = null;
        binaryLoaded = false;
        binaryDidLoad = false;

        if (csvNumberOfFramesPerPerson != null) csvNumberOfFramesPerPerson.Clear();
        if (timeFrameToIndex != null) timeFrameToIndex.Clear();
        if (peopleIndexes != null) peopleIndexes.Clear();
        if (personsOriginal != null) personsOriginal.Clear();
        if (persons != null) persons.Clear();
        if (personsRecord != null) personsRecord.Clear();

        if(skeletonsList != null) skeletonsList.Clear();
        if (skeletonJoints != null) skeletonJoints.Clear();
        if (rigidAvatars != null) rigidAvatars.Clear();

        numberOfPesonsFromCSVLoad = 0;
        count = 0;
        countPlay = 0;
        currentTime = 0;
        initSimulationTime = -1.0f;

        allFramesAndPersonsFromCSVLoad = 0; //number of lines of the cvd
        dataPerPersonAndFrameFromCSVLoad = 0; // number of columns
        numberOfPesonsFromCSVLoad = 0; // total number of persons   
        initialTime = float.MaxValue;
        endingTime = float.MinValue;

        lastCountPlay = -1;

    
    int frameStartIndex;

}
    static public List<Transform> getJoints(GameObject skeleton) {

        List<Transform> joints = new List<Transform>();

        if (skeleton.transform.Find("Skeleton") != null)
        {
            //find joints by name 
            joints.Add(skeleton.transform.FindDeepChild("Skeleton"));

            joints.Add(skeleton.transform.FindDeepChild("Hips"));
            joints.Add(skeleton.transform.FindDeepChild("Chest"));
            joints.Add(skeleton.transform.FindDeepChild("Chest2"));
            joints.Add(skeleton.transform.FindDeepChild("Chest3"));
            joints.Add(skeleton.transform.FindDeepChild("Chest4"));

            joints.Add(skeleton.transform.FindDeepChild("Neck"));
            joints.Add(skeleton.transform.FindDeepChild("Head"));
            joints.Add(skeleton.transform.FindDeepChild("HeadSite"));

            joints.Add(skeleton.transform.FindDeepChild("RightCollar"));
            joints.Add(skeleton.transform.FindDeepChild("RightShoulder"));
            joints.Add(skeleton.transform.FindDeepChild("RightElbow"));
            joints.Add(skeleton.transform.FindDeepChild("RightWrist"));
            joints.Add(skeleton.transform.FindDeepChild("RightWristSite"));

            joints.Add(skeleton.transform.FindDeepChild("LeftCollar"));
            joints.Add(skeleton.transform.FindDeepChild("LeftShoulder"));
            joints.Add(skeleton.transform.FindDeepChild("LeftElbow"));
            joints.Add(skeleton.transform.FindDeepChild("LeftWrist"));
            joints.Add(skeleton.transform.FindDeepChild("LeftWristSite"));

            joints.Add(skeleton.transform.FindDeepChild("RightHip"));
            joints.Add(skeleton.transform.FindDeepChild("RightKnee"));
            joints.Add(skeleton.transform.FindDeepChild("RightAnkle"));
            joints.Add(skeleton.transform.FindDeepChild("RightToe"));
            joints.Add(skeleton.transform.FindDeepChild("RightToeSite"));

            joints.Add(skeleton.transform.FindDeepChild("LeftHip"));
            joints.Add(skeleton.transform.FindDeepChild("LeftKnee"));
            joints.Add(skeleton.transform.FindDeepChild("LeftAnkle"));
            joints.Add(skeleton.transform.FindDeepChild("LeftToe"));
            joints.Add(skeleton.transform.FindDeepChild("LeftToeSite"));

        }
        else if (skeleton.transform.Find("Bip01") != null)
        {

            //find joints by name 
            joints.Add(skeleton.transform.FindDeepChild("Bip01"));

            joints.Add(skeleton.transform.FindDeepChild("Bip01_Pelvis"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_Spine"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_Spine1"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_Spine2"));
            joints.Add(skeleton.transform.FindDeepChild("")); //null

            joints.Add(skeleton.transform.FindDeepChild("Bip01_Neck"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_Head"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_HeadNub"));

            joints.Add(skeleton.transform.FindDeepChild("")); //null
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_UpperArm"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Forearm"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Hand"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Finger0"));

            joints.Add(skeleton.transform.FindDeepChild("")); //null
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_UpperArm"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Forearm"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Hand"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Finger0"));

            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Thigh"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Calf"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Foot"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Toe0"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_L_Toe0Nub"));

            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Thigh"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Calf"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Foot"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Toe0"));
            joints.Add(skeleton.transform.FindDeepChild("Bip01_R_Toe0Nub"));

        }
        else
        {

            Debug.Log("Bone structure unkwnown -- no joints added");
        }

        return joints;
    }
    public int getJointsNumber(GameObject Skeleton) {

        int number = getJoints(Skeleton).Count;

        return number;

    }
    public float GetInitialTime()
    {
        return initialTime;
    }
    public void UpdateSkeletons(List<GameObject> sks)
    {
        skeletonsList = new List<GameObject>(sks);
        seconds = endingTime - initialTime;
        skeletonNumbers = skeletonsList.Count;

        numberOfValuesPerFrame = (jointsNumbers+1) * (timeAndIndex + positionCoord); // the + one on the joints number is related to position 

        List<GameObject> skeletons = new List<GameObject>(sks);
        skeletonJoints = new List<List<Transform>>();
        int index = 0;
        foreach (GameObject skeleton in skeletons)
        {
            
            List<Transform> joints = getJoints(skeleton);

            skeletonJoints.Add(joints);

            // need for new serialization
            SIGGRAPH_2017.BioAnimation_Adam_Simulation component = skeleton.GetComponent<SIGGRAPH_2017.BioAnimation_Adam_Simulation>();
            if (component != null)
            {
                component.InitWithCSVData(index, timeStep);
            }
            index++;
        }

        //coordinates = new float[total];
        coordinates = new float[0];
        Setup();
    }
    public void UpdateRigidAvatars(List<GameObject> ra)
    {
        
        rigidAvatars = new List<Transform>();

        foreach (GameObject rigidAvatar in ra)
        {          
            rigidAvatars.Add(rigidAvatar.transform);
        }
        
        Setup();
    }

    void Setup()
    {
        if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.RECORD) 
        {
            Time.captureDeltaTime = 1.0f / framerate;
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAYCSV)
        {
            
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAY)
        {
            /*n1 = skeletonNumbers * (jointsNumbers + 1) * (timeAndIndex + positionCoord);
            n2 = (jointsNumbers + 1) * (timeAndIndex + positionCoord);
            n3 = (timeAndIndex + positionCoord);

            LoadDatasetTest();*/
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.LOADED)
        {
            n1 = skeletonNumbers * (jointsNumbers + 1) * (timeAndIndex + positionCoord);
            n2 = (jointsNumbers + 1) * (timeAndIndex + positionCoord);
            n3 = (timeAndIndex + positionCoord);

            LoadDatasetTest();
        }

    }
    void Update()
    {
        if (!CSVLoaded()) return;
        if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.RECORD)
        {
            SimulationManagerAdam.Instance.sceneLoaded = true;
            DelegatedCumulateData();
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAYCSV)
        {
            SimulationManagerAdam.Instance.sceneLoaded = true;
            ReadDataPerFrameCsv();
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAY)
        {
            if (BinaryLoaded())
            {
                SimulationManagerAdam.Instance.sceneLoaded = true;
                ReadDataFromSimulationPerFrame();
            }
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.LOADED)
        {
            if (BinaryLoaded())
            {
                SimulationManagerAdam.Instance.sceneLoaded = true;
            }
        }
    }
    void LoadDatasetTest()
    {
       StartCoroutine(DeserializeOnAndroid());
    }

    void SerializeAll()
    {
        if (serializationDone) return;
        serializationDone = true;

        int[] framesPerSkeleton = new int[skeletonsList.Count];
        int[] frameIndexPerSkeleton = new int[skeletonsList.Count];
        int maximum = (int)((endingTime - initialTime) / timeStep);
        int totalPerSkeleton = maximum * numberOfValuesPerFrame;

        coordinates = new float[maximum * skeletonsList.Count * numberOfValuesPerFrame];
        
        long count = 0;
        for (float t =initialTime; t<endingTime; t+=timeStep) 
        {
            for (int j = 0; j < skeletonsList.Count; j++)
            {
                
                AnimationInputHandlerFromAdamSimulation component1 = skeletonsList[j].GetComponent<AnimationInputHandlerFromAdamSimulation>();
                SIGGRAPH_2017.BioAnimation_Adam_Simulation component2 = skeletonsList[j].GetComponent<SIGGRAPH_2017.BioAnimation_Adam_Simulation>();
                if(frameIndexPerSkeleton[j] < component1.timedPositions.Count && Math.Abs(component1.timedPositions[frameIndexPerSkeleton[j]].time - t) < 0.01f ) 
                {
                    for (int k = 0; k < (jointsNumbers + 1) ; k++) //plus one is for position and rotation twice 
                    {
                        int baseIndex = frameIndexPerSkeleton[j] * (jointsNumbers+1) * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length || baseIndex >= component2.coordsToSerialize.Length) break;

                        if (k == 0) //plus one is for position and rotation twice 
                        {
                            coordinates[count] = component2.coordsToSerialize[baseIndex];
                            coordinates[count + 1] = component2.coordsToSerialize[baseIndex + 1];
                            coordinates[count + 2] = component2.coordsToSerialize[baseIndex + 2];
                            coordinates[count + 3] = component2.coordsToSerialize[baseIndex + 3];

                            k++;
                            baseIndex = frameIndexPerSkeleton[j] * (jointsNumbers+1) * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                            count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);

                            coordinates[count] = component2.coordsToSerialize[baseIndex];
                            coordinates[count + 1] = component2.coordsToSerialize[baseIndex + 1];
                            coordinates[count + 2] = component2.coordsToSerialize[baseIndex + 2];
                            coordinates[count + 3] = component2.coordsToSerialize[baseIndex + 3];                        

                        }
                        else
                        {
                            coordinates[count] = component2.coordsToSerialize[baseIndex];
                            coordinates[count + 1] = component2.coordsToSerialize[baseIndex + 1];
                            coordinates[count + 2] = component2.coordsToSerialize[baseIndex + 2];
                            coordinates[count + 3] = component2.coordsToSerialize[baseIndex + 3];
                        }

                        count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);
                    }
                    frameIndexPerSkeleton[j] += 1;
                } else
                {
                    for (int k = 0; k < (jointsNumbers +1); k++)
                    {
                        //int baseIndex = frameIndexPerSkeleton[j] * jointsNumbers * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length) break;
                        //Debug.Log("Time:" + t.ToString() + " Skeleton:" + j.ToString() + " count:" + count + " total:" + coordinates.Length);
                        
                        if (k == 0) //plus one is for position and rotation twice 
                        {
                            coordinates[count] = t;
                            coordinates[count + 1] = float.NaN;
                            coordinates[count + 2] = float.NaN;
                            coordinates[count + 3] = float.NaN;

                            k++;                           
                            count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);

                            coordinates[count] = float.NaN;
                            coordinates[count + 1] = float.NaN;
                            coordinates[count + 2] = float.NaN;
                            coordinates[count + 3] = float.NaN;
                        }
                        else
                        {
                            coordinates[count] = float.NaN;
                            coordinates[count + 1] = float.NaN;
                            coordinates[count + 2] = float.NaN;
                            coordinates[count + 3] = float.NaN;
                        }

                        count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);
                    }
                }
            }
        }

        Serialize();
    }
    void Serialize() //used in UNITY_EDITOR, SO path should be UNITY_EDITOR
    {
        Debug.Log("Begin Of Serialisation");

        FileStream fs = new FileStream(Path.Combine(path, datafile), FileMode.Create);

        // Construct a BinaryFormatter and use it to serialize the data to the stream.
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            formatter.Serialize(fs, coordinates);
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to serialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }

        Debug.Log("End Of Serialisation");

        /*int xprecision = 5;
        string formatString = "{0:G" + xprecision + "}\t{1:G" + xprecision + "}\t{2:G" + xprecision + "}\t{3:G" + xprecision + "}";

        using (var outf = new StreamWriter(Path.Combine(path, txtFileName)))
            for (int i = 0; i < coordinates.Length; i=i+4)
                outf.WriteLine(formatString, coordinates[i], coordinates[i+1], coordinates[i+2], coordinates[i+3]);*/
         
    }

    public void LoadFromCSV()
    {
        StartCoroutine(DeserializeFromCSVOnAndroid());
    }

    void ConversionFromPositionsToVariations()
    {
        // this function changes the data loaded from CSV ans stored in csvCoordinates
        // in a similar array where position data is converted in variation from frame to frame
        // example 
        // from
        // 0 0.1 x0 y0
        // 0 0.2 x1 y1 
        // to 
        // 0 0.1 x1-x0 y1-y0
        // 0 0.2 x2-x1 x2-x1
        csvVariationsCoordinates = new float[csvCoordinates.Length];
        int cumulativeFrames = 0;
        for (int i=0; i< numberOfPesonsFromCSVLoad; i++)
        {
            //Debug.Log((new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber().ToString());
            int numberOfFramesFromCSVLoad = csvNumberOfFramesPerPerson[i];
            cumulativeFrames += i != 0 ? csvNumberOfFramesPerPerson[i-1] : 0;
            for (int j = 0; j < numberOfFramesFromCSVLoad; j++)
            {
                //Debug.Log((new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber().ToString() + " frame:" + j.ToString() + " person:" + i.ToString() + " on total:" + numberOfPesonsFromCSVLoad.ToString());
                int index = cumulativeFrames * dataPerPersonAndFrameFromCSVLoad + dataPerPersonAndFrameFromCSVLoad * j;
                csvVariationsCoordinates[index] = csvCoordinates[index];
                csvVariationsCoordinates[index+1] = csvCoordinates[index+1];
                int xPosIndex = index + 2;
                int yPosIndex = index + 3;
                if (j == 1) {
                    timeStep = (float)System.Math.Round(csvCoordinates[index + 7] - csvCoordinates[index + 7 - dataPerPersonAndFrameFromCSVLoad], precisionFloatLoad);
                }
                else if (j != numberOfFramesFromCSVLoad-1)
                {
                    csvVariationsCoordinates[xPosIndex] = csvCoordinates[xPosIndex + dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[xPosIndex];
                    csvVariationsCoordinates[yPosIndex] = csvCoordinates[yPosIndex + dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[yPosIndex];
                }
                else
                {
                    csvVariationsCoordinates[xPosIndex] = 0.0f; //last , no variation
                    csvVariationsCoordinates[yPosIndex] = 0.0f;
                    
                }
                csvVariationsCoordinates[index + 4] = csvCoordinates[index + 4];
                csvVariationsCoordinates[index + 5] = csvCoordinates[index + 5];
                csvVariationsCoordinates[index + 6] = csvCoordinates[index + 6];
                csvVariationsCoordinates[index + 7] = csvCoordinates[index + 7];
                persons[i][csvVariationsCoordinates[index + 7]] = new Vector2(csvVariationsCoordinates[xPosIndex], csvVariationsCoordinates[yPosIndex]);
                AnimationInputHandlerFromAdamSimulation.TimedPosition t = new AnimationInputHandlerFromAdamSimulation.TimedPosition();
                t.time = csvCoordinates[index + 7];
                t.position = new Vector2(csvCoordinates[index + 2], csvCoordinates[index + 3]);
                t.direction = new Vector2(csvVariationsCoordinates[xPosIndex], csvVariationsCoordinates[yPosIndex]);
                //Debug.Log((new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber().ToString() + " p:" + personsRecord.Count.ToString() + " i:" + i.ToString() );
                personsRecord[i].Add(t);
                
            }

        }
        CalculateInitialAndEndingTime();
    }

    public void CalculateInitialAndEndingTime()
    {
        for(int i = 0; i < personsOriginal.Count; i++)
        {
            float localMin = personsOriginal[i].Keys.Min();
            if (localMin < initialTime)
            {
                initialTime = localMin;
            }

            float localMax = personsOriginal[i].Keys.Max();
            if (localMax > endingTime)
            {
                endingTime = localMax;
            }
        }
        simulationTimeLength = (endingTime - initialTime);
    }

    public static float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array, i * 4, 4);
            }
            floatArr[i] = BitConverter.ToSingle(array, i * 4);
        }
        return floatArr;
    }

    IEnumerator DeserializeOnAndroid()
    {
        www = new WWW(Path.Combine(path, datafile));
        yield return www;
        binaryLoaded = true;
        
    }

    public bool BinaryLoaded()
    {
        if (binaryDidLoad == true) return true;
        if (binaryLoaded == false) return false;

        if (www.bytes.Length == 0) return false;
        binaryDidLoad = true;

        MemoryStream ms = new MemoryStream(www.bytes);
        //Debug.Log("Data size: " + www.bytes.Length.ToString());
        BinaryFormatter formatter = new BinaryFormatter();
        coordinates = (float[])formatter.Deserialize(ms);
        ms.Close();
        //MapTimeFrameToIndex();
        return true;
    }

    public void DeserializeCSV()
    {
        numberOfPesonsFromCSVLoad = 0;

        CrowdCSVReader reader = new CrowdCSVReader();
        reader.Load(Path.Combine(path, csvFileName));
        List<CrowdCSVReader.Row> list = reader.GetRowList();
        //

        int numberOfColumns = 8;
        dataPerPersonAndFrameFromCSVLoad = numberOfColumns;

        int numberOfTotalElements = list.Count * numberOfColumns; //number of columns is 8
        allFramesAndPersonsFromCSVLoad = list.Count;

        csvCoordinates = new float[numberOfTotalElements];
        csvNumberOfFramesPerPerson = new List<int>();

        int count = 0;
        int lastId = -1;
        int frameCount = 0;

        foreach (CrowdCSVReader.Row row in list)
        {
            csvCoordinates[count] = row.id != "" ? float.Parse(row.id) : float.MinValue;

            if (lastId != (int)csvCoordinates[count])
            {
                numberOfPesonsFromCSVLoad += 1;
                lastId = (int)csvCoordinates[count];

                peopleIndexes.Add(count);
                persons.Add(new Dictionary<float, Vector2>());
                personsOriginal.Add(new Dictionary<float, Vector2>());
                personsRecord.Add(new List<AnimationInputHandlerFromAdamSimulation.TimedPosition>());

                if (count != 0)
                {
                    csvNumberOfFramesPerPerson.Add(frameCount);
                    frameCount = 0;
                }

            }

            csvCoordinates[count + 1] = row.gid != "" ? float.Parse(row.gid) : float.MinValue;
            csvCoordinates[count + 2] = row.x != "" ? float.Parse(row.x) * scaleValue : float.MinValue;
            csvCoordinates[count + 3] = row.y != "" ? float.Parse(row.y) * scaleValue : float.MinValue;
            csvCoordinates[count + 4] = row.dir_x != "" ? float.Parse(row.dir_x) * scaleValue : float.MinValue;
            csvCoordinates[count + 5] = row.dir_y != "" ? float.Parse(row.dir_y) * scaleValue : float.MinValue;
            csvCoordinates[count + 6] = row.radius != "" ? float.Parse(row.radius) * scaleValue : float.MinValue;
            csvCoordinates[count + 7] = row.time != "" ? (float)System.Math.Round(float.Parse(row.time), precisionFloatLoad) : float.MinValue;

            personsOriginal[personsOriginal.Count - 1][csvCoordinates[count + 7]] = new Vector2(csvCoordinates[count + 2], csvCoordinates[count + 3]);

            count += numberOfColumns;
            frameCount += 1;
        }

        csvNumberOfFramesPerPerson.Add(frameCount); //add the last

        ConversionFromPositionsToVariations();
        CalculateInitialAndEndingTime();
    }

    IEnumerator DeserializeFromCSVOnAndroid()
    {
        file = new WWW(Path.Combine(path, csvFileName));
        yield return file;
        csvLoaded = true;
    }

    public bool CSVLoaded()
    {
        if (csvDidLoad == true) return true;
        if (csvLoaded == false) return false;

        csvDidLoad = true;

        csvAsset = new TextAsset(file.text);
        CrowdCSVReader reader = new CrowdCSVReader();
        reader.Load(csvAsset);
        List<CrowdCSVReader.Row> list = reader.GetRowList();
        //

        int numberOfColumns = 8;
        dataPerPersonAndFrameFromCSVLoad = numberOfColumns;

        int numberOfTotalElements = list.Count * numberOfColumns; //number of columns is 8
        allFramesAndPersonsFromCSVLoad = list.Count;

        csvCoordinates = new float[numberOfTotalElements];
        csvNumberOfFramesPerPerson = new List<int>();

        int count = 0;
        int lastId = -1;
        int frameCount = 0;

        foreach (CrowdCSVReader.Row row in list)
        {
            csvCoordinates[count] = row.id != "" ? float.Parse(row.id) : float.MinValue;

            if (lastId != (int)csvCoordinates[count])
            {
                numberOfPesonsFromCSVLoad += 1;
                lastId = (int)csvCoordinates[count];

                peopleIndexes.Add(count);
                persons.Add(new Dictionary<float, Vector2>());
                personsOriginal.Add(new Dictionary<float, Vector2>());
                personsRecord.Add(new List<AnimationInputHandlerFromAdamSimulation.TimedPosition>());
                if (count != 0)
                {
                    csvNumberOfFramesPerPerson.Add(frameCount);
                    frameCount = 0;
                }

            }

            csvCoordinates[count + 1] = row.gid != "" ? float.Parse(row.gid) : float.MinValue;
            csvCoordinates[count + 2] = row.x != "" ? float.Parse(row.x) * scaleValue : float.MinValue;
            csvCoordinates[count + 3] = row.y != "" ? float.Parse(row.y) * scaleValue : float.MinValue;
            csvCoordinates[count + 4] = row.dir_x != "" ? float.Parse(row.dir_x) * scaleValue : float.MinValue;
            csvCoordinates[count + 5] = row.dir_y != "" ? float.Parse(row.dir_y) * scaleValue : float.MinValue;
            csvCoordinates[count + 6] = row.radius != "" ? float.Parse(row.radius) * scaleValue : float.MinValue;
            csvCoordinates[count + 7] = row.time != "" ? (float)System.Math.Round(float.Parse(row.time), precisionFloatLoad) : float.MinValue;

            personsOriginal[personsOriginal.Count - 1][csvCoordinates[count + 7]] = new Vector2(csvCoordinates[count + 2], csvCoordinates[count + 3]);

            count += numberOfColumns;
            frameCount += 1;
        }

        csvNumberOfFramesPerPerson.Add(frameCount); //add the last

        ConversionFromPositionsToVariations();
        CalculateInitialAndEndingTime();
#if UNITY_EDITOR
        SimulationManagerAdam.Instance.ResizeScene();
#endif
        if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.LOADED)
        {
            SimulationManagerAdam.Instance.currentTime = 0.0f;
            SimulationManagerAdam.Instance.OnStartPlay();
        } else if(SimulationManagerAdam.status == SimulationManagerAdam.STATUS.RECORD)
        {
            currentTime = 0.0f;
            SimulationManagerAdam.Instance.OnStartRecord();
        }

        return true;
    }



    public void ReadDataFromSimulationPerFrame() // rewrite the function with a different cumulative data that take in account the timeframe
    {
        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.time;
        }

        float currentSimulationTime = (Time.time - initSimulationTime);
        if (currentSimulationTime >= simulationTimeLength)
        {
            return;
        }


        countPlay = (int)(currentSimulationTime / timeStep);
        
        if(lastCountPlay == countPlay)
        {
            return;
        } else
        {
            lastCountPlay = countPlay;
        }


        frameStartIndex = countPlay * n1; //countplay maximum is seconds * framerate 
        

        for (int s = 0; s < skeletonNumbers; s++)
        {
            GameObject parent = skeletonJoints[s][0].parent.gameObject;

            for (int j = 0; j < jointsNumbers; j++)
            {

                if (skeletonJoints[s][j] == null)
                {
                    if (j == 0) j++;
                    continue;
                }

                if (j == 0)
                {

                    v[0] = frameStartIndex + s * n2 + j * n3;
                    v[1] = v[0] + 1;
                    v[2] = v[0] + 2;
                    v[3] = v[0] + 3;

                    v[4] = frameStartIndex + s * n2 + (j + 1) * n3;
                    v[5] = v[4] + 1;
                    v[6] = v[4] + 2;
                    v[7] = v[4] + 3;

                    bool visible = v[1] >= coordinates.Length || float.IsNaN(coordinates[v[1]]) ? false : true;

                    parent.SetActive(visible);

                    if (!visible) break;

                    skeletonJoints[s][j].parent.transform.position = new Vector3(coordinates[v[1]], coordinates[v[2]], coordinates[v[3]]);
                    skeletonJoints[s][j].parent.transform.rotation = new Quaternion(coordinates[v[4]], coordinates[v[5]], coordinates[v[6]], coordinates[v[7]]);

                }
                else
                {

                    v[0] = frameStartIndex + s * n2 + (j + 1) * n3;
                    v[1] = v[0] + 1;
                    v[2] = v[0] + 2;
                    v[3] = v[0] + 3;

                    bool visible = v[1] >= coordinates.Length || float.IsNaN(coordinates[v[1]]) ? false : true;

                    parent.SetActive(visible);

                    if (!visible) break;

                    skeletonJoints[s][j].localRotation = new Quaternion(coordinates[v[0]], coordinates[v[1]], coordinates[v[2]], coordinates[v[3]]);
                }

            }
            
        }

        countPlay += 1;

    }

    public void ReadDataPerFrameCsv() // rewrite the function with a different cumulative data that take in account the timeframe
    {

        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.fixedUnscaledTime;
        }

        float currenttime = (Time.fixedUnscaledTime - initSimulationTime);
    

        for (int i = 0; i < persons.Count; ++i)
        {
            

            float closest = persons[i]
                .Select(n => new { n, distance = Math.Abs( n.Key- currenttime) })
                .OrderBy(p => p.distance)
                .First().n.Key;

            if (Math.Abs(closest - currenttime) < 0.1)
            {
                rigidAvatars[i].gameObject.SetActive(true);
                rigidAvatars[i].transform.position = new Vector3(personsOriginal[i][closest][0], rigidAvatars[i].transform.position.y, personsOriginal[i][closest][1]);
            }
            else {
                rigidAvatars[i].gameObject.SetActive(false);
            }
        }
    }
    
    void DelegatedCumulateData()
    {
        if (skeletonsList.Count == 0) return;
        bool lastSerialization = true;
        currentTime += Time.deltaTime;

        for (int i = 0; i < skeletonsList.Count; i++)
        {
            AnimationInputHandlerFromAdamSimulation component = skeletonsList[i].GetComponent<AnimationInputHandlerFromAdamSimulation>();
            //component.SetActiveTime(currentTime);
            if (!component.isArrived())
            {
                lastSerialization = false;
            }
        }

        if (forceSerialization)
        {
            for (int i = 0; i < skeletonsList.Count; i++)
            {
                skeletonsList[i].SetActive(false);
            }
        }

        if (lastSerialization || forceSerialization)
        {
            SerializeAll(); // serialize when finished
        }
    }
}
