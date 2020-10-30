using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System;

public class PositionSerializer : MonoBehaviour
{
    public List<List<Transform>> skeletonJoints;

    public List<Transform> rigidAvatars;
    List<GameObject> skeletonsList;

    public string csvFileName;
    private int count = 0;
    private int countPlay = 0;
    public float scaleValue = 1f;

    private int seconds = 60;
    private int framerate = 72;
    private int skeletonNumbers = 1; //change to the real number or move code to Start.
    public static int jointsNumbers = 32;
    public static int timeAndIndex = 2;
    public static int positionCoord = 3;
    
    private float[] coordinates;
    private int timeTotal;
    private int total;
    public static int totalPerSkeleton;
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
    public List<List<AnimationInputHandlerFromSimulation.TimedPosition>> personsRecord = new List<List<AnimationInputHandlerFromSimulation.TimedPosition>>();
    public float timeStep;
    [HideInInspector]
    public float initialTime = float.MaxValue;
    [HideInInspector]
    public float endingTime = float.MinValue;

    TextAsset csvAsset;

    // Start is called before the first frame update
    void Start()
    {
        //Init();
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

    public void UpdateSkeletons(List<GameObject> sks)
    {
        skeletonsList = new List<GameObject>(sks);

        timeTotal = seconds * framerate;
        skeletonNumbers = skeletonsList.Count;

        numberOfValuesPerFrame = jointsNumbers * (timeAndIndex + positionCoord);
        totalPerSkeleton = seconds * framerate * numberOfValuesPerFrame;
        total = totalPerSkeleton * skeletonNumbers;

        List<GameObject> skeletons = new List<GameObject>(sks);
        skeletonJoints = new List<List<Transform>>();
        int index = 0;
        foreach (GameObject skeleton in skeletons)
        {
            Transform[] children = skeleton.transform.GetComponentsInChildren<Transform>();
            List<Transform> joints = new List<Transform>(children);
            skeletonJoints.Add(joints);

            // need for new serialization
            SIGGRAPH_2017.BioAnimation_Simulation component = skeleton.GetComponent<SIGGRAPH_2017.BioAnimation_Simulation>();
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

        timeTotal = seconds * framerate;

        Setup();
    }

    void Setup()
    {
        if (SimulationManager.status == SimulationManager.STATUS.RECORD) 
        {
            Time.captureDeltaTime = 1.0f / framerate;
        }
        else if (SimulationManager.status == SimulationManager.STATUS.PLAYCSV)
        {
            //anything here?
        }
        else if (SimulationManager.status == SimulationManager.STATUS.PLAY)
        {
            LoadDatasetTest();
        }

    }

    void Update()
    {
        if (SimulationManager.status == SimulationManager.STATUS.RECORD)
        {
            //CumulateData();
            DelegatedCumulateData();
        }
        else if (SimulationManager.status == SimulationManager.STATUS.PLAYCSV)
        {
            ReadDataPerFrameCsv();
        }
        else if (SimulationManager.status == SimulationManager.STATUS.PLAY)
        {
            if (fileLoaded)
            {
                ReadDataFromSimulationPerFrame();
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
        // merge all the coords
        //@@TODO
        //timeStep
        //initialTime
        //endingTime

        int[] framesPerSkeleton = new int[skeletonsList.Count];
        int[] frameIndexPerSkeleton = new int[skeletonsList.Count];
        //int totalNumberOfData = 0;
        int maximum = 0;
        for (int j=0; j <skeletonsList.Count; j++)
        {
            AnimationInputHandlerFromSimulation component = skeletonsList[j].GetComponent<AnimationInputHandlerFromSimulation>();
            framesPerSkeleton[j] = component.timedPositions.Count;
            if(maximum < framesPerSkeleton[j])
            {
                maximum = framesPerSkeleton[j];
            }
            //totalNumberOfData += framesPerSkeleton[j] * numberOfValuesPerFrame;
        }

        coordinates = new float[maximum * skeletonsList.Count * numberOfValuesPerFrame];

        int count = 0;
        for (float t = 0.0f; t<=endingTime; t+=timeStep) // weeird issue with initialTime @@TODO
        {
            for (int j = 0; j < skeletonsList.Count; j++)
            {
                AnimationInputHandlerFromSimulation component1 = skeletonsList[j].GetComponent<AnimationInputHandlerFromSimulation>();
                SIGGRAPH_2017.BioAnimation_Simulation component2 = skeletonsList[j].GetComponent<SIGGRAPH_2017.BioAnimation_Simulation>();
                if(frameIndexPerSkeleton[j] < component1.timedPositions.Count && Math.Abs(component1.timedPositions[frameIndexPerSkeleton[j]].time - t) < 0.0001f ) 
                {
                    for (int k = 0; k < jointsNumbers; k++)
                    {
                        int baseIndex = frameIndexPerSkeleton[j] * jointsNumbers * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length) break;

                        coordinates[count] = component2.coordsToSerialize[baseIndex];
                        coordinates[count + 1] = component2.coordsToSerialize[baseIndex+1];
                        coordinates[count + 2] = component2.coordsToSerialize[baseIndex+2];
                        coordinates[count + 3] = component2.coordsToSerialize[baseIndex+3];
                        coordinates[count + 4] = component2.coordsToSerialize[baseIndex+4];
                        count += 5;
                    }
                    frameIndexPerSkeleton[j] += 1;
                } else
                {
                    for (int k = 0; k < jointsNumbers; k++)
                    {
                        //int baseIndex = frameIndexPerSkeleton[j] * jointsNumbers * (timeAndIndex + positionCoord) + k * (timeAndIndex + positionCoord);
                        if (count >= coordinates.Length) break;
                        coordinates[count] = t;
                        coordinates[count + 1] = j;
                        coordinates[count + 2] = float.NaN;
                        coordinates[count + 3] = float.NaN;
                        coordinates[count + 4] = float.NaN;
                        count += 5;
                    }
                }
                //component.coordsToSerialize
            }
        }

        
        
        //call Serialize
        Serialize();
        serializationDone = true;
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

        /*int xprecision = 3;
        string formatString = "{0:G" + xprecision + "}\t{1:G" + xprecision + "}\t{2:G" + xprecision + "}\t{3:G" + xprecision + "}\t{4:G" + xprecision + "}";

        using (var outf = new StreamWriter(Path.Combine(path, "DataFile.txt")))
            for (int i = 0; i < coordinates.Length; i=i+5)
                outf.WriteLine(formatString, coordinates[i], coordinates[i+1], coordinates[i+2], coordinates[i+3], coordinates[i+4]);*/
        
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

        int maximum = 0;
        for (int j = 0; j < skeletonsList.Count; j++)
        {
            framesPerSkeleton[j] = personsOriginal[j].Count;
            if (maximum < framesPerSkeleton[j])
            {
                maximum = framesPerSkeleton[j];
            }
            //totalNumberOfData += framesPerSkeleton[j] * numberOfValuesPerFrame;
        }
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

                personsRecord.Add(new List<AnimationInputHandlerFromSimulation.TimedPosition>());

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
            int numberOfFramesFromCSVLoad = csvNumberOfFramesPerPerson[i];
            cumulativeFrames += i != 0 ? csvNumberOfFramesPerPerson[i-1] : 0;
            for (int j = 0; j < numberOfFramesFromCSVLoad; j++)
            {
                int index = cumulativeFrames * dataPerPersonAndFrameFromCSVLoad + dataPerPersonAndFrameFromCSVLoad * j;
                csvVariationsCoordinates[index] = csvCoordinates[index];
                csvVariationsCoordinates[index+1] = csvCoordinates[index+1];
                int xPosIndex = index + 2;
                int yPosIndex = index + 3;

                if(j != numberOfFramesFromCSVLoad-1)
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
                AnimationInputHandlerFromSimulation.TimedPosition t = new AnimationInputHandlerFromSimulation.TimedPosition();
                t.time = csvCoordinates[index + 7];
                t.position = new Vector2(csvCoordinates[index + 2], csvCoordinates[index + 3]);
                t.direction = new Vector2(csvVariationsCoordinates[xPosIndex], csvVariationsCoordinates[yPosIndex]);
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

        SimulationManager.Instance.currentTimeStep = timeStep;
        SimulationManager.Instance.currentTime = 0.0f;
        SimulationManager.Instance.OnStartPlay();
    }
    
    public void ReadDataFromSimulationPerFrame() // rewrite the function with a different cumulative data that take in account the timeframe
    {
        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.fixedUnscaledTime;
            
        }

        //float currentRatio = (Time.fixedUnscaledTime - initSimulationTime) / seconds;
        //countPlay = (int)System.Math.Round(timeTotal * currentRatio, System.MidpointRounding.AwayFromZero);
        float currenttime = (Time.fixedUnscaledTime - initSimulationTime);
        //conversion
        //  ------------------------------------------- 1 second (30)
        //                                  ----------- 1 frame (72)
        //                                        ----- 1 sekeleton (60)
        //                                          --- 1 joint (32)
        //                                            - 1 coordinate (3)
        //int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord); //countplay maximum is seconds * framerate 
        int frameStartIndex = timeFrameToIndex[0]
               .Select(n => new { n, distance = Math.Abs(n.Key - currenttime) })
               .OrderBy(p => p.distance)
               .First().n.Value;
         

        for (int s = 0; s < skeletonNumbers; s++)
        {
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + s * jointsNumbers * (timeAndIndex + positionCoord) + j * (timeAndIndex + positionCoord);//the first skeleton s = 0 // if you want andom put s = and the number of recorded skeletons
                int indexX = baseIndex + 2;
                int indexY = baseIndex + 3;
                int indexZ = baseIndex + 4;

                if (indexX >= coordinates.Length || float.IsNaN(coordinates[indexX]))
                {
                    skeletonJoints[s][j].gameObject.SetActive(false);
                }
                else
                {
                    skeletonJoints[s][j].gameObject.SetActive(true);
                    skeletonJoints[s][j].position = new Vector3(coordinates[indexX], coordinates[indexY], coordinates[indexZ]);
                }

            }
        }

        countPlay += 1;
        /*coordinates[count] = sj.position.x;
        coordinates[count + 1] = sj.position.y;
        coordinates[count + 2] = sj.position.z;
        count += 3;*/

    }

    public void ReadDataPerFrame() // rewrite the function with a different cumulative data that take in account the timeframe
    {

        if(initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.fixedUnscaledTime;
        }

        float currentRatio = (Time.fixedUnscaledTime - initSimulationTime) / seconds;
        countPlay = (int)System.Math.Round(timeTotal * currentRatio, System.MidpointRounding.AwayFromZero);
        

        if (countPlay >= timeTotal)
        {
            return;
        }

        
        //conversion
        //  ------------------------------------------- 1 second (30)
        //                                  ----------- 1 frame (72)
        //                                        ----- 1 sekeleton (60)
        //                                          --- 1 joint (32)
        //                                            - 1 coordinate (3)
        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord); //countplay maximum is seconds * framerate 
        //int frameEndIndex = (countPlay+1) * skeletonNumbers * jointsNumbers * positionCoord - 1;
        //skeleton indexes
        for (int s = 0; s < skeletonNumbers; s++)
        {
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + 0 * jointsNumbers * (timeAndIndex + positionCoord) + j * (timeAndIndex + positionCoord);//the first skeleton s = 0 // if you want andom put s = and the number of recorded skeletons
                int indexX = baseIndex;
                int indexY = baseIndex + 1;
                int indexZ = baseIndex + 2;
                skeletonJoints[s][j].localPosition = new Vector3(coordinates[indexX], coordinates[indexY], coordinates[indexZ]);
            }
        }

        countPlay += 1;
        /*coordinates[count] = sj.position.x;
        coordinates[count + 1] = sj.position.y;
        coordinates[count + 2] = sj.position.z;
        count += 3;*/

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

    public void ReadFirstPerFrameOnMultipleSkeletons()
    {
        if (initSimulationTime == -1.0f)
        {
            initSimulationTime = Time.fixedTime; //before was UnscaledTime
        }

        float currentRatio = (Time.fixedUnscaledTime - initSimulationTime) / seconds;
        countPlay = (int)System.Math.Round(timeTotal * currentRatio, System.MidpointRounding.AwayFromZero);


        if (countPlay >= timeTotal)
        {
            return;
        }


        //conversion
        //  ------------------------------------------- 1 second (30)
        //                                  ----------- 1 frame (72)
        //                                        ----- 1 sekeleton (60)
        //                                          --- 1 joint (32)
        //                                            - 1 coordinate (3)
        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * (timeAndIndex + positionCoord); //countplay maximum is seconds * framerate 
        //int frameEndIndex = (countPlay + 1) * skeletonNumbers * jointsNumbers * positionCoord - 1;
        //skeleton indexes

        int currentSkeletonNumber = GameObject.FindGameObjectsWithTag("skeleton").Length;
        for (int s = 0; s < currentSkeletonNumber; s++)
        {
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + s * jointsNumbers * (timeAndIndex + positionCoord) + j * (timeAndIndex + positionCoord);
                int indexX = baseIndex;
                int indexY = baseIndex + 1;
                int indexZ = baseIndex + 2;
                skeletonJoints[s][j].localPosition = new Vector3(coordinates[indexX], coordinates[indexY], coordinates[indexZ]);
            }
        }

        countPlay += 1;
        /*coordinates[count] = sj.position.x;
        coordinates[count + 1] = sj.position.y;
        coordinates[count + 2] = sj.position.z;
        count += 3;*/

    }

    void DelegatedCumulateData()
    {
        //Debug.Log(skeletons.Count);
        bool lastSerialization = true;

        //if (initSimulationTime == -1.0f)
        //{
        //    initSimulationTime = Time.fixedTime;
        //}

        currentTime += Time.deltaTime;

        for (int i=0; i< skeletonsList.Count; i++)
        {
            AnimationInputHandlerFromSimulation component = skeletonsList[i].GetComponent<AnimationInputHandlerFromSimulation>();
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

    void CumulateData() //maybe check if skeleton is active or enable with simulation data
    {
        //Debug.Log(skeletons.Count);
        if (count == total)
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
    }
    
}
