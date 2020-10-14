using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class PositionSerializer : MonoBehaviour
{
    public List<List<Transform>> skeletonJoints;
    private int count = 0;
    private int countPlay = 0;

    private static int seconds = 30;
    private static int framerate = 72;
    private static int skeletonNumbers = 2; //change to the real number or move code to Start.
    private static int jointsNumbers = 32;
    private static int positionCoord = 3;
    private static int timeTotal = seconds * framerate;
    private static int total = seconds* framerate * skeletonNumbers* jointsNumbers * positionCoord;
    public float[] coordinates;

    string path;
    bool record = false;
    bool play = false;


    float initSimulationTime = -1.0f;
    //string path = @"C:\Users\dannox\Desktop\crowdCount\CrowdVR\UnityCrowdVR\";

    bool end = false;
    // Start is called before the first frame update
    void Start()
    {

        Init();


        // enable this to record ...
        // store data and serialize to a file
        //RecordData();

        // or this to play...
        // load th dataset from file and then Play it
        LoadDatasetTest();
        PlayData(); // this play like a map between the same number of skeletons in record and play phase
        
    }

    void Init()
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
        
        coordinates = new float[total];

        GameObject[] sks = GameObject.FindGameObjectsWithTag("skeleton");
        List<GameObject> skeletons = new List<GameObject>(sks);
        skeletonJoints = new List<List<Transform>>();
        foreach (GameObject skeleton in skeletons)
        {
            Transform[] children = skeleton.transform.GetComponentsInChildren<Transform>();
            List<Transform> joints = new List<Transform>(children);
            skeletonJoints.Add(joints);
        }
    }

    void Update()
    {
        if (record)
        {
            CumulateData();
        }

        if(play)
        {
            //ReadDataPerFrame(); //mapping N to N
            ReadFirstPerFrameOnMultipleSkeletons(); 
        }
    }


    void RecordData()
    {
        record = true;
        Time.captureDeltaTime = 1.0f / framerate;
    }

    void PlayData()
    {
        play = true;
    }


    void LoadDatasetTest()
    {
#if UNITY_EDITOR
        Deserialize(); // UNITY_EDITOR
#elif UNITY_ANDROID
        StartCoroutine(DeserializeOnAndroid());
#endif
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
            coordinates = (float [])formatter.Deserialize(fs);
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
    }

    IEnumerator DeserializeOnAndroid()
    {
        WWW file = new WWW(Path.Combine(path, "DataFile.dat"));
        yield return file;
        MemoryStream ms = new MemoryStream(file.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        coordinates = (float[])formatter.Deserialize(ms);
        ms.Close();
    }



    void ReadDataPerFrame()
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
        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * positionCoord; //countplay maximum is seconds * framerate 
        //int frameEndIndex = (countPlay+1) * skeletonNumbers * jointsNumbers * positionCoord - 1;
        //skeleton indexes
        for (int s = 0; s < skeletonNumbers; s++)
        {
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + 0 * jointsNumbers * positionCoord + j * positionCoord;//the first skeleton s = 0 // if you want andom put s = and the number of recorded skeletons
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

    void ReadFirstPerFrameOnMultipleSkeletons()
    {
        if (initSimulationTime == -1.0f)
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
        int frameStartIndex = countPlay * skeletonNumbers * jointsNumbers * positionCoord; //countplay maximum is seconds * framerate 
        //int frameEndIndex = (countPlay + 1) * skeletonNumbers * jointsNumbers * positionCoord - 1;
        //skeleton indexes

        int currentSkeletonNumber = GameObject.FindGameObjectsWithTag("skeleton").Length;
        for (int s = 0; s < currentSkeletonNumber; s++)
        {
            for (int j = 0; j < jointsNumbers; j++)
            {
                int baseIndex = frameStartIndex + s * jointsNumbers * positionCoord + j * positionCoord;
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

    void CumulateData()
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

        foreach (List<Transform> sjl in skeletonJoints)
        {
            foreach (Transform sj in sjl)
            {
                coordinates[count] = sj.localPosition.x;
                coordinates[count + 1] = sj.localPosition.y;
                coordinates[count + 2] = sj.localPosition.z;
                count += 3;
            }
        }
    }
}
