using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataCache : MonoBehaviour
{
    // Start is called before the first frame update
    static public Dictionary<string, float[]> cacheBinary = new Dictionary<string, float[]>();
    static public Dictionary<string, float[]> cacheCSV = new Dictionary<string, float[]>();

    private static DataCache instance = null;
    public string[] datanames;
    public float[] scaleValue;
    int numberOfColumns = 8;

    List<WWW> file = new List<WWW>();
    List<bool> csvLoaded = new List<bool> { false, false };
    List<bool> csvDidLoad = new List<bool> { false, false };

    List<WWW> www = new List<WWW>();
    List<bool> binaryLoaded = new List<bool> { false, false };
    List<bool> binaryDidLoad = new List<bool> { false, false };

    public class CSVInfo
    {
        public string dataname;
        public int dataPerPersonAndFrameFromCSVLoad = 0;
        public int allFramesAndPersonsFromCSVLoad = 0;
        public int numberOfPesonsFromCSVLoad = 0;
        public List<int> csvNumberOfFramesPerPerson;
        public List<Dictionary<float, Vector2>> personsOriginal = new List<Dictionary<float, Vector2>>();
        public List<List<AnimationInputHandlerFromAdamSimulation.TimedPosition>> personsRecord = new List<List<AnimationInputHandlerFromAdamSimulation.TimedPosition>>();
        public float initialTime = float.MaxValue;
        public float endingTime = float.MinValue;
        public Vector2 initialOrientation;
        public float simulationTimeLength;
        public float timeStep;
    };
    List<CSVInfo> infos = new List<CSVInfo>();

    public CSVInfo GetInfo(string dataname)
    {
        return infos.Where(p => p.dataname == dataname).Single();
    }

    string path;
    public bool allLoaded = false;

    public static DataCache Instance
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
    }

    private void Start()
    {
        path = Application.streamingAssetsPath;

        for (int i = 0; i < datanames.Length; i++)
        {
            TransitionManager.Instance.setWaitingView();
            www.Add(null);
            StartCoroutine(DeserializeOnAndroid(i));
            CSVInfo info = new CSVInfo();
            info.dataname = datanames[i];
            infos.Add(info);
            file.Add(null);
            StartCoroutine(DeserializeFromCSVOnAndroid(i));
        }
    }

    IEnumerator DeserializeOnAndroid(int index)
    {
        www[index] = new WWW(Path.Combine(path, datanames[index] + ".dat"));
        yield return www[index];
        binaryLoaded[index] = true;
    }

    IEnumerator DeserializeFromCSVOnAndroid(int index)
    {
        file[index] = new WWW(Path.Combine(path, datanames[index] + ".csv"));
        yield return file[index];
        csvLoaded[index] = true;
    }

    public bool BinaryLoaded(int index)
    {
        if (binaryDidLoad[index] == true) return true;
        if (binaryLoaded[index] == false) return false;

        if (www[index].bytes.Length == 0) return false;
        binaryDidLoad[index] = true;

        MemoryStream ms = new MemoryStream(www[index].bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        float[] bin = ((float[])formatter.Deserialize(ms));
        SetDataInCache(datanames[index], ref bin);

        ms.Close();
        return true;
    }

    public bool CSVLoaded(int index)
    {
        if (csvDidLoad[index] == true) return true;
        if (csvLoaded[index] == false) return false;

        csvDidLoad[index] = true;
        
        TextAsset csvAsset = new TextAsset(file[index].text);
        CrowdCSVReader reader = new CrowdCSVReader();
        reader.Load(csvAsset);
        List<CrowdCSVReader.Row> list = reader.GetRowList();
        //
        infos[index].dataPerPersonAndFrameFromCSVLoad = numberOfColumns;

        int numberOfTotalElements = list.Count * numberOfColumns; //number of columns is 8
        infos[index].allFramesAndPersonsFromCSVLoad = list.Count;

        float[] csvCoordinates = new float[numberOfTotalElements];
        SetCSVInCache(datanames[index], ref csvCoordinates);
        infos[index].csvNumberOfFramesPerPerson = new List<int>();

        int count = 0;
        int lastId = -1;
        int frameCount = 0;
        for (int m = 0; m < list.Count; m++)
        {
            csvCoordinates[count] = list[m].id != "" ? float.Parse(list[m].id) : float.MinValue;

            if (lastId != (int)csvCoordinates[count])
            {
                infos[index].numberOfPesonsFromCSVLoad += 1;
                lastId = (int)csvCoordinates[count];

                infos[index].personsOriginal.Add(new Dictionary<float, Vector2>());
                infos[index].personsRecord.Add(new List<AnimationInputHandlerFromAdamSimulation.TimedPosition>());
                if (count != 0)
                {
                    infos[index].csvNumberOfFramesPerPerson.Add(frameCount);
                    frameCount = 0;
                }

            }

            csvCoordinates[count + 1] = list[m].gid != "" ? float.Parse(list[m].gid) : float.MinValue;
            csvCoordinates[count + 2] = list[m].x != "" ? float.Parse(list[m].x) * scaleValue[index] : float.MinValue;
            csvCoordinates[count + 3] = list[m].y != "" ? float.Parse(list[m].y) * scaleValue[index] : float.MinValue;
            csvCoordinates[count + 4] = list[m].dir_x != "" ? float.Parse(list[m].dir_x) * scaleValue[index] : float.MinValue;
            csvCoordinates[count + 5] = list[m].dir_y != "" ? float.Parse(list[m].dir_y) * scaleValue[index] : float.MinValue;
            csvCoordinates[count + 6] = list[m].radius != "" ? float.Parse(list[m].radius) * scaleValue[index] : float.MinValue;
            csvCoordinates[count + 7] = list[m].time != "" ? (float)System.Math.Round(float.Parse(list[m].time), 3) : float.MinValue; //precisionfloat

            infos[index].personsOriginal[infos[index].personsOriginal.Count - 1][csvCoordinates[count + 7]] = new Vector2(csvCoordinates[count + 2], csvCoordinates[count + 3]);

            count += numberOfColumns;
            frameCount += 1;
        }

        infos[index].csvNumberOfFramesPerPerson.Add(frameCount); //add the last

        ConversionFromPositionsToVariations(index);
        CalculateInitialAndEndingTime(index);

        //#if UNITY_EDITOR
        //SimulationManagerAdam.Instance.ResizeSceneNoDestroy();
        //#endif

        /*if (SimulationManagerAdam.status == SimulationManagerAdam.STATUS.LOADED)
        {
            SimulationManagerAdam.Instance.currentTime = 0.0f;
            SimulationManagerAdam.Instance.OnStartPlay();
        }*/
       

        return true;
    }

    void ConversionFromPositionsToVariations(int v)
    {
        float[] csvCoordinates;
        GeCSVFromCache(datanames[v], out csvCoordinates);
         
        int cumulativeFrames = 0;
        for (int i = 0; i < infos[v].numberOfPesonsFromCSVLoad; i++)
        {
            int numberOfFramesFromCSVLoad = infos[v].csvNumberOfFramesPerPerson[i];
            cumulativeFrames += i != 0 ? infos[v].csvNumberOfFramesPerPerson[i - 1] : 0;
            for (int j = 0; j < numberOfFramesFromCSVLoad; j++)
            {
                int index = cumulativeFrames * infos[v].dataPerPersonAndFrameFromCSVLoad + infos[v].dataPerPersonAndFrameFromCSVLoad * j;

                int xPosIndex = index + 2;
                int yPosIndex = index + 3;
                float dir_x = 0.0f;
                float dir_y = 0.0f;
                if (j == 1)
                {
                    infos[v].timeStep = (float)System.Math.Round(csvCoordinates[index + 7] - csvCoordinates[index + 7 - infos[v].dataPerPersonAndFrameFromCSVLoad], 3); //precision is 3
                    dir_x = csvCoordinates[xPosIndex + infos[v].dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[xPosIndex];
                    dir_y = csvCoordinates[yPosIndex + infos[v].dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[yPosIndex];
                }
                else if (j != numberOfFramesFromCSVLoad - 1)
                {
                    dir_x = csvCoordinates[xPosIndex + infos[v].dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[xPosIndex];
                    dir_y = csvCoordinates[yPosIndex + infos[v].dataPerPersonAndFrameFromCSVLoad] - csvCoordinates[yPosIndex];

                }
                
                AnimationInputHandlerFromAdamSimulation.TimedPosition t = new AnimationInputHandlerFromAdamSimulation.TimedPosition();
                t.time = csvCoordinates[index + 7];
                t.position = new Vector2(csvCoordinates[index + 2], csvCoordinates[index + 3]);
                t.direction = new Vector2(dir_x, dir_y);
                infos[v].personsRecord[i].Add(t);

            }

        }
    }

    public void CalculateInitialAndEndingTime(int index)
    {
        for (int i = 0; i < infos[index].personsOriginal.Count; i++)
        {
            float localMin = infos[index].personsOriginal[i].Keys.Min();
            if (localMin < infos[index].initialTime)
            {
                infos[index].initialTime = localMin;
            }

            float localMax = infos[index].personsOriginal[i].Keys.Max();
            if (localMax > infos[index].endingTime)
            {
                infos[index].endingTime = localMax;
            }
        }
        infos[index].simulationTimeLength = (infos[index].endingTime - infos[index].initialTime);
    }

    public bool IsDataCached(string dataname)
    {
        return cacheBinary.ContainsKey(dataname);
    }
    
    public void SetDataInCache(string dataname, ref float[] array)
    {
        if(cacheBinary.ContainsKey(dataname))
        {
            return;
        } else
        {
            cacheBinary[dataname] = array;
        }
    }

    public bool GetDataFromCache(string dataname, out float[] array)
    {
        if (cacheBinary.ContainsKey(dataname))
        {
            array = cacheBinary[dataname];
            return true;
        }
        array = new float[0];
        return false;
    }

    public void SetCSVInCache(string dataname, ref float[] array)
    {

        if (cacheCSV.ContainsKey(dataname))
        {
            return;
        }
        else
        {
            cacheCSV[dataname] = array;
        }
    }

    public bool GeCSVFromCache(string dataname, out float[] array)
    {
        if (cacheCSV.ContainsKey(dataname))
        {
            array = cacheCSV[dataname];
            return true;
        }
        array = new float[0];
        return false;
    }



    // Update is called once per frame
    void Update()
    {
        bool all = true;
        for(int i=0; i<datanames.Length; i++)
        {
            all = CSVLoaded(i) && BinaryLoaded(i) && all;
        }
        allLoaded = all;
        //Debug.Log(allLoaded);
        if(allLoaded)
        {
            TransitionManager.Instance.setExperimentView();
        }
        
    }
}
