using System.IO;
using UnityEngine;
using System.Collections;
using System;
using TableTop;

public class AvatarBehaviourRecorder : MonoBehaviour
{
    StreamWriter writer;

    string line;

    Vector3 PointerHead1 = Vector3.zero;

    Vector3 PointerHead2 = Vector3.zero;

    Vector3 PointerHand1 = Vector3.zero;

    Vector3 PointerHand2 = Vector3.zero;

    string PointerHead1string;
    string PointerHead2string;
    string PointerHand1string;
    string PointerHand2string;


    string LocalHeadPos ;
    string RemoteHeadPos;
    string ControllerPos;
    string RemoteControllerPos;

    string LocalHeadEAng;
    string RemoteHeadEAng;
    string ControllerEAng;
    string RemoteControllerEAng;

    inputsManager inputsmanagerinstance;

    public int frameRate = 25;

    float startTime;
    
    float currentTime;

    Char[] remove = new Char[] { ' ', '(', ')' };

    GameObject cube;

    RayOnMap rayOnMap;
    private void OnEnable()
    {


        Time.captureFramerate = frameRate;

    }

    void Update()
    {

        if (writer == null ) return;

        LocalHeadPos = inputsmanagerinstance.LocalHead == null ? "null,null,null" : inputsmanagerinstance.LocalHead.position.ToString("F3");
        RemoteHeadPos = inputsmanagerinstance.RemoteHead == null ? "null,null,null" : inputsmanagerinstance.RemoteHead.position.ToString("F3");      
        ControllerPos = inputsmanagerinstance.Controller == null ? "null,null,null" : inputsmanagerinstance.Controller.position.ToString("F3");
        RemoteControllerPos = inputsmanagerinstance.RemoteController == null ? "null,null,null" : inputsmanagerinstance.RemoteController.position.ToString("F3");

        LocalHeadEAng = inputsmanagerinstance.LocalHead == null ? "null,null,null" : inputsmanagerinstance.LocalHead.eulerAngles.ToString("F3");
        RemoteHeadEAng = inputsmanagerinstance.RemoteHead == null ? "null,null,null" : inputsmanagerinstance.RemoteHead.eulerAngles.ToString("F3");
        ControllerEAng = inputsmanagerinstance.Controller == null ? "null,null,null" : inputsmanagerinstance.Controller.eulerAngles.ToString("F3");
        RemoteControllerEAng = inputsmanagerinstance.RemoteController == null ? "null,null,null" : inputsmanagerinstance.RemoteController.eulerAngles.ToString("F3");


        currentTime = Time.unscaledTime - startTime;

        line = currentTime.ToString("F3") + ","+
        LocalHeadPos.Trim(remove) + "," + LocalHeadEAng.Trim(remove) + "," +
        ControllerPos.Trim(remove) + "," + ControllerEAng.Trim(remove) + "," +
        PointerHead1string.Trim(remove) + "," + PointerHand1string.Trim(remove) + "," +
        RemoteHeadPos.Trim(remove) + "," + RemoteHeadEAng.Trim(remove) + "," +
        RemoteControllerPos.Trim(remove) + "," + RemoteControllerEAng.Trim(remove) + "," +
        PointerHead2string.Trim(remove) + "," + PointerHand1string.Trim(remove);

        writer.WriteLine(line);

    }

    public void NewData(GameObject g) {

        closeWriter();

        if (inputsmanagerinstance == null) inputsmanagerinstance = inputsManager.Instance;

        string path = Application.dataPath + "\\" + MasterManager.GameSettings.DataFolder + "\\" + g.name + ".csv";
        writer = new StreamWriter(path, true);

        writer.WriteLine("time in s, LocalHeadX, LocalHeadY, LocalHeadZ,LocalHeadEulerX, LocalHeadEulerY, LocalHeadEulerZ, ControllerX, ControllerY, ControllerZ,ControllerEulerX, ControllerEulerY, ControllerEulerZ,PointerHead1X,PointerHead1Y,PointerHead1Z,PointerHand1X,PointerHand1Y,PointerHand1Z," +
           "RemoteHeadX, RemoteHeadY, RemoteHeadZ,RemoteHeadEulerX, RemoteHeadEulerY, RemoteHeadEulerZ, RemoteControllerX, RemoteontrollerY, RemoteControllerZ,ControllerEulerX, RemoteControllerEulerY, RemoteControllerEulerZ,PointerHead2X,PointerHead2Y,PointerHead2Z,PointerHand2X,PointerHand2Y,PointerHand2Z");

        startTime = Time.unscaledTime;

    }

    void OnDisable()
    {
        closeWriter();
    }

    void OnApplicationQuit()
    {
        closeWriter(); 
    }

    public void closeWriter() {

        if (writer != null) writer.Close();

        writer = null;

    }

    RayOnMap GetRayOnMap() {

        RayOnMap[] list = FindObjectsOfType<RayOnMap>();

        foreach (RayOnMap r in list)
        {

            if (r.gameObject.activeSelf)
            {

                rayOnMap = r;

                return r;
            }
        }

        return null;
    }
}
