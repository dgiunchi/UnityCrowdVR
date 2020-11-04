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
    public List<List<Transform>> skeletonJoints;

    public List<Transform> rigidAvatars;
    List<GameObject> skeletonsList;

    public string csvFileName;
    private int count = 0;
    private int countPlay = 0;
    public float scaleValue = 1f;

    private float seconds;
    [HideInInspector]
    public static int framerate = 72;
    private int skeletonNumbers = 1; //change to the real number or move code to Start.
    public static int jointsNumbers = 32;
    public static int timeAndIndex = 2;
    public static int positionCoord = 7;
    
    private float[] coordinates;
    
    public static int numberOfValuesPerFrame;

    string path;
    float currentTime = 0;
    float initSimulationTime = -1.0f;
    //string path = @"C:\Users\dannox\Desktop\crowdCount\CrowdVR\UnityCrowdVR\";

    bool fileLoaded = false;
    bool end = false;
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

    TextAsset csvAsset;

    // Start is called before the first frame update
    void Start()
    {
        //Init();
        jointsNumbers = SimulationManagerAdam.Instance.skeletonRecordPrefab.transform.GetComponentsInChildren<Transform>().Length - 2; //no root object and no mesh.
         
        
    }

    public void Init()
    {
        //path = Application.persistentDataPath;
        path = Application.streamingAssetsPath; // valid for UNITY_EDITOR and UNITY_ANDROID

        /*DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);
        Debug.Log("--------------Streaming Assets Path: " + Application.streamingAssetsPath);
        FileInfo[] allFiles = directoryInfo.GetFiles("*.*");
        for(int i=0; i<allFiles.Length; i++)
        {
            Debug.Log(allFiles[i].Name);
        }*/
        
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

        numberOfValuesPerFrame = jointsNumbers * (timeAndIndex + positionCoord);

        List<GameObject> skeletons = new List<GameObject>(sks);
        skeletonJoints = new List<List<Transform>>();
        int index = 0;
        foreach (GameObject skeleton in skeletons)
        {
            Transform[] children = skeleton.transform.GetComponentsInChildren<Transform>().Skip(2).ToArray(); // root and mesh to be skipped
            List<Transform> joints = new List<Transform>(children);
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
            //anything here?
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAY)
        {
            
            LoadDatasetTest();
        }

    }

    void Update()
    {
        if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.RECORD)
        {
            DelegatedCumulateData();
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAYCSV)
        {
            ReadDataPerFrameCsv();
        }
        else if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.PLAY)
        {
            if (fileLoaded)
            {
                if(SimulationManagerAdam.Instance.singlePlay)
                {
                    ReadSingleDataFromSimulationPerFrame(SimulationManagerAdam.Instance.indexPlay);
                } else
                {
                    
                    ReadDataFromSimulationPerFrame();
                }
                
            }
            
            //ReadDataPerFrame(); //mapping N to N
            //ReadFirstPerFrameOnMultipleSkeletons();
        }

    }


    void LoadDatasetTest()
    {
#if UNITY_EDITOR
        Deserialize(); // UNITY_EDITOR
#elif UNITY_ANDROID
       
        StartCoroutine(DeserializeOnAndroid());
#endif
    }

   
    void SerializeAll()
    {
        if (serializationDone) return;
        serializationDone = true;
        // merge all the coords
        //@@TODO
        //timeStep
        //initialTime
        //endingTime

        int[] framesPerSkeleton = new int[skeletonsList.Count];
        int[] frameIndexPerSkeleton = new int[skeletonsList.Count];
        //int totalNumberOfData = 0;
        int maximum = (int)((endingTime - initialTime) / timeStep);
        int totalPerSkeleton = maximum * numberOfValuesPerFrame;

        coordinates = new float[maximum * skeletonsList.Count * numberOfValuesPerFrame];
        
        long count = 0;
        for (float t =initialTime; t<endingTime; t+=timeStep) // weeird issue with initialTime @@TODO
        {
            for (int j = 0; j < skeletonsList.Count; j++)
            {
                
                AnimationInputHandlerFromAdamSimulation component1 = skeletonsList[j].GetComponent<AnimationInputHandlerFromAdamSimulation>();
                SIGGRAPH_2017.BioAnimation_Adam_Simulation component2 = skeletonsList[j].GetComponent<SIGGRAPH_2017.BioAnimation_Adam_Simulation>();
                if(frameIndexPerSkeleton[j] < component1.timedPositions.Count && Math.Abs(component1.timedPositions[frameIndexPerSkeleton[j]].time - t) < 0.01f ) 
                {
                    for (int k = 0; k < jointsNumbers; k++)
                    {
                        int baseIndex = frameIndexPerSkeleton[j] * jointsNumbers * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length || baseIndex >= component2.coordsToSerialize.Length) break;
                        //Debug.Log("Time:" + t.ToString() + " Skeleton:" + j.ToString() + " count:" + count + " total:" + coordinates.Length);
                        coordinates[count] = component2.coordsToSerialize[baseIndex];
                        coordinates[count + 1] = component2.coordsToSerialize[baseIndex+1];
                        coordinates[count + 2] = component2.coordsToSerialize[baseIndex+2];
                        coordinates[count + 3] = component2.coordsToSerialize[baseIndex+3];
                        coordinates[count + 4] = component2.coordsToSerialize[baseIndex+4];
                        coordinates[count + 5] = component2.coordsToSerialize[baseIndex + 5];
                        coordinates[count + 6] = component2.coordsToSerialize[baseIndex + 6];
                        coordinates[count + 7] = component2.coordsToSerialize[baseIndex + 7];
                        coordinates[count + 8] = component2.coordsToSerialize[baseIndex + 8];
                        count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);
                    }
                    frameIndexPerSkeleton[j] += 1;
                } else
                {
                    for (int k = 0; k < jointsNumbers; k++)
                    {
                        //int baseIndex = frameIndexPerSkeleton[j] * jointsNumbers * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length) break;
                        //Debug.Log("Time:" + t.ToString() + " Skeleton:" + j.ToString() + " count:" + count + " total:" + coordinates.Length);
                        coordinates[count] = t;
                        coordinates[count + 1] = j;
                        coordinates[count + 2] = float.NaN;
                        coordinates[count + 3] = float.NaN;
                        coordinates[count + 4] = float.NaN;
                        coordinates[count + 5] = float.NaN;
                        coordinates[count + 6] = float.NaN;
                        coordinates[count + 7] = float.NaN;
                        coordinates[count + 8] = float.NaN;
                        count += (PositionSerializerAdam.timeAndIndex + PositionSerializerAdam.positionCoord);
                    }
                }
                //component.coordsToSerialize
            }
        }

        
        
        //call Serialize
        Serialize();
    }

    void Serialize() //used in UNITY_EDITOR, SO path should be UNITY_EDITOR
    {
        Debug.Log("Begin Of Serialisation");
        
        FileStream fs = new FileStream(Path.Combine(path, "DataFile.dat"), FileMode.Create);

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
        string formatString = "{0:G" + xprecision + "}\t{1:G" + xprecision + "}\t{2:G" + xprecision + "}\t{3:G" + xprecision + "}\t{4:G" + xprecision + "}\t{5:G" + xprecision + "}\t{6:G" + xprecision + "}\t{7:G" + xprecision + "}\t{8:G" + xprecision + "}";

        using (var outf = new StreamWriter(Path.Combine(path, "DataFile.txt")))
            for (int i = 0; i < coordinates.Length; i=i+9)
                outf.WriteLine(formatString, coordinates[i], coordinates[i+1], coordinates[i+2], coordinates[i+3], coordinates[i+4], coordinates[i + 5], coordinates[i +6], coordinates[i + 7], coordinates[i + 8]);
        */
    }

    void Deserialize()
    {
        Debug.Log("Begin Of Deserialisation");
        FileStream fs = new FileStream(Path.Combine(path, "DataFile.dat"), FileMode.Open);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and
            // assign the reference to the local variable.
            coordinates = (float[]) ((float [])formatter.Deserialize(fs)).Clone();
        }
        catch (SerializationException e)
        {
            Debug.Log("Failed to deserialize. Reason: " + e.Message);
            throw;
        }
        finally
        {
            fs.Close();
        }
        Debug.Log("End Of Deserialisation");
        MapTimeFrameToIndex();
        fileLoaded = true;
    }

    void MapTimeFrameToIndex()
    {
        int[] framesPerSkeleton = new int[skeletonsList.Count];


        /*for (int j = 0; j < skeletonsList.Count; j++)
        {
            framesPerSkeleton[j] = personsOriginal[j].Count;
            if (maximum < framesPerSkeleton[j])
            {
                maximum = framesPerSkeleton[j];
            }
            //totalNumberOfData += framesPerSkeleton[j] * numberOfValuesPerFrame;
        }*/
        int maximum = (int)((endingTime - initialTime) / timeStep);
        int totalPerSkeleton = maximum * numberOfValuesPerFrame;

        timeFrameToIndex = new List<Dictionary<float, int>>();

        for (int i = 0; i < skeletonsList.Count; ++i)
        {
            float currentTime = 0;
            Dictionary<float, int> dict = new Dictionary<float, int>();
            for(int j=0; j< totalPerSkeleton; ++j )
            {
                dict[(float)Math.Round(currentTime, 2)] = j * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord);
                currentTime += timeStep;
            }
            timeFrameToIndex.Add(dict);
        }
    }



    public void LoadFromCSV()
    {
#if UNITY_EDITOR
        DeserializeFromCSV();
        ConversionFromPositionsToVariations();
        CalculateInitialAndEndingTime();
              
#elif UNITY_ANDROID
        StartCoroutine(DeserializeFromCSVOnAndroid());
#endif
    }

    void DeserializeFromCSV()
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
            
            if(lastId != (int) csvCoordinates[count])
            {
                numberOfPesonsFromCSVLoad += 1;
                lastId = (int) csvCoordinates[count];

                peopleIndexes.Add(count);
                persons.Add(new Dictionary<float, Vector2>());
                personsOriginal.Add(new Dictionary<float, Vector2>());
                personsRecord.Add(new List<AnimationInputHandlerFromAdamSimulation.TimedPosition>());

                if (count!=0)
                {
                    csvNumberOfFramesPerPerson.Add(frameCount);
                    frameCount = 0;
                }
                
            }

            csvCoordinates[count + 1] = row.gid != "" ? float.Parse(row.gid) : float.MinValue;
            csvCoordinates[count + 2] = row.x != "" ? float.Parse(row.x)* scaleValue : float.MinValue ;
            csvCoordinates[count + 3] = row.y != "" ? float.Parse(row.y)* scaleValue : float.MinValue ;
            csvCoordinates[count + 4] = row.dir_x != "" ? float.Parse(row.dir_x) * scaleValue : float.MinValue;
            csvCoordinates[count + 5] = row.dir_y != "" ? float.Parse(row.dir_y)* scaleValue : float.MinValue ;
            csvCoordinates[count + 6] = row.radius != "" ? float.Parse(row.radius)* scaleValue : float.MinValue ;
            csvCoordinates[count + 7] = row.time != "" ? (float)System.Math.Round(float.Parse(row.time),2) : float.MinValue;

            personsOriginal[personsOriginal.Count - 1][csvCoordinates[count + 7]] = new Vector2(csvCoordinates[count + 2], csvCoordinates[count + 3]);

            count += numberOfColumns;
            frameCount += 1;
        }

        csvNumberOfFramesPerPerson.Add(frameCount); //add the last

        
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
                if (j != numberOfFramesFromCSVLoad-1)
                {
                    csvVariationsCoordinates[xPosIndex] = csvCoordinates[xPosIndex + dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[xPosIndex];
                    csvVariationsCoordinates[yPosIndex] = csvCoordinates[yPosIndex + dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[yPosIndex];
                } else
                {
                    csvVariationsCoordinates[xPosIndex] = 0.0f; //last , no variation
                    csvVariationsCoordinates[yPosIndex] = 0.0f;

                    // calculation of timestep
                    timeStep = (float) System.Math.Round(csvCoordinates[index + 7] - csvCoordinates[index + 7 - dataPerPersonAndFrameFromCSVLoad],2);
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



    IEnumerator DeserializeOnAndroid()
    {
        WWW file = new WWW(Path.Combine(path, "DataFile.dat"));
        yield return file;
        MemoryStream ms = new MemoryStream(file.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        coordinates = (float[])formatter.Deserialize(ms);
        ms.Close();
        MapTimeFrameToIndex();
        fileLoaded = true;
    }

    IEnumerator DeserializeFromCSVOnAndroid()
    {
        WWW file = new WWW(Path.Combine(path, "crowds_zara01_corrected.csv"));
        yield return file;
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
            csvCoordinates[count + 2] = row.x != "" ? float.Parse(row.x) : float.MinValue;
            csvCoordinates[count + 3] = row.y != "" ? float.Parse(row.y) : float.MinValue;
            csvCoordinates[count + 4] = row.dir_x != "" ? float.Parse(row.dir_x) : float.MinValue;
            csvCoordinates[count + 5] = row.dir_y != "" ? float.Parse(row.dir_y) : float.MinValue;
            csvCoordinates[count + 6] = row.radius != "" ? float.Parse(row.radius) : float.MinValue;
            csvCoordinates[count + 7] = row.time != "" ? float.Parse(row.time) : float.MinValue;

            personsOriginal[personsOriginal.Count - 1][csvCoordinates[count + 7]] = new Vector2(csvCoordinates[count + 2], csvCoordinates[count + 3]);

            count += numberOfColumns;
            frameCount += 1;
        }

        csvNumberOfFramesPerPerson.Add(frameCount); //add the last

        ConversionFromPositionsToVariations();
        CalculateInitialAndEndingTime();

        SimulationManagerAdam.Instance.currentTimeStep = timeStep;
        SimulationManagerAdam.Instance.currentTime = 0.0f;
        SimulationManagerAdam.Instance.OnStartPlay();
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
            initSimulationTime = -1.0f;
            return;
        }


        countPlay = (int)System.Math.Round(currentSimulationTime / timeStep, System.MidpointRounding.AwayFromZero);
        
        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord); //countplay maximum is seconds * framerate 
        

        for (int s = 0; s < skeletonNumbers; s++)
        {
            GameObject parent = skeletonJoints[s][0].parent.gameObject;
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + s * jointsNumbers * (timeAndIndex + positionCoord) + j * (timeAndIndex + positionCoord);//the first skeleton s = 0 // if you want andom put s = and the number of recorded skeletons
                int indexX = baseIndex + 2;
                int indexY = baseIndex + 3;
                int indexZ = baseIndex + 4;
                int indexXR = baseIndex + 5;
                int indexYR = baseIndex + 6;
                int indexZR = baseIndex + 7;
                int indexWR = baseIndex + 8;

                if (indexX >= coordinates.Length || float.IsNaN(coordinates[indexX]))
                {
                    parent.SetActive(false);
                }
                else
                {
                    parent.SetActive(true);
                    skeletonJoints[s][j].position = Vector3.Lerp(skeletonJoints[s][j].position, new Vector3(coordinates[indexX], coordinates[indexY], coordinates[indexZ]), Time.deltaTime /timeStep);
                    skeletonJoints[s][j].rotation = Quaternion.Lerp(skeletonJoints[s][j].rotation, new Quaternion(coordinates[indexXR], coordinates[indexYR], coordinates[indexZR], coordinates[indexWR]), Time.deltaTime / timeStep);
                }

            }
        }

        countPlay += 1;


    }

    public void ReadSingleDataFromSimulationPerFrame(int index) // rewrite the function with a different cumulative data that take in account the timeframe
    {
        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.time;
        }

        float currentSimulationTime = (Time.time - initSimulationTime);
        if (currentSimulationTime >= simulationTimeLength)
        {
            initSimulationTime = -1.0f;
            return;
        }

        countPlay = (int)System.Math.Round(currentSimulationTime / timeStep, System.MidpointRounding.AwayFromZero);

        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord); //countplay maximum is seconds * framerate 
        
        for (int s = 0; s < skeletonNumbers; s++)
        {
            GameObject parent = skeletonJoints[s][0].parent.gameObject;
            if (index != s)
            {
                parent.SetActive(false);
                continue;
            }

            
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + s * jointsNumbers * (timeAndIndex + positionCoord) + j * (timeAndIndex + positionCoord);//the first skeleton s = 0 // if you want andom put s = and the number of recorded skeletons
                int indexX = baseIndex + 2;
                int indexY = baseIndex + 3;
                int indexZ = baseIndex + 4;
                int indexXR = baseIndex + 5;
                int indexYR = baseIndex + 6;
                int indexZR = baseIndex + 7;
                int indexWR = baseIndex + 8;

                if (indexX >= coordinates.Length || float.IsNaN(coordinates[indexX]))
                {
                    parent.SetActive(false);
                }
                else
                {
                    parent.SetActive(true);
                    skeletonJoints[s][j].position = Vector3.Lerp(skeletonJoints[s][j].position, new Vector3(coordinates[indexX], coordinates[indexY], coordinates[indexZ]), Time.deltaTime / timeStep);
                    skeletonJoints[s][j].rotation = Quaternion.Lerp(skeletonJoints[s][j].rotation, new Quaternion(coordinates[indexXR], coordinates[indexYR], coordinates[indexZR], coordinates[indexWR]), Time.deltaTime / timeStep);
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

        //

    }
    
    void DelegatedCumulateData()
    {
        
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

        if (lastSerialization)
        {
            SerializeAll(); // serialize when finished
        }
    }

    /*void CumulateData()  // @@TOREMOV
    {
        if (currentTime = total)
        {
            if(end == false)
            {
                Serialize(); // serialize when finished
                end = true;
            }
            return;
        }

        currentTime += Time.deltaTime;
        int skeletonCount = 0;
        foreach (List<Transform> sjl in skeletonJoints)
        {
            foreach (Transform sj in sjl)
            {
                coordinates[count] = currentTime;
                coordinates[count + 1] = skeletonCount;
                if (sj.gameObject.activeInHierarchy == true)
                {
                    coordinates[count + 2] = sj.localPosition.x;
                    coordinates[count + +3] = sj.localPosition.y;
                    coordinates[count + +4] = sj.localPosition.z;
                } else
                {
                    coordinates[count + 2] = float.NaN;
                    coordinates[count + +3] = float.NaN;
                    coordinates[count + +4] = float.NaN;
                }
                
                count += 5;
            }
            skeletonCount++;
        }
    }*/
    
}
