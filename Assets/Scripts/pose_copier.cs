using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class pose_copier : MonoBehaviour
{
    public bool init = false;
    private GameObject Target;
    private List<Transform> TargetJoints;
    private List<Quaternion?> TargetOrigins;
    public GameObject target {

        set { 
            Target = value;
        }
        get {

            return Target;
        }

    }

    public GameObject Source;
    private List<Transform> SourceJoints;
    private List<Quaternion?> SourceOrigins;
    public GameObject source
    {

        set
        {
            Source = value;
          
            
        }
        get {
            return Source;
        }

    }

    Dictionary<string, string> d = new Dictionary<string, string>();
    public void initialize()
    {
        Target = GameObject.Find("Proxy_avatar_rocket_box");       
        SetJoints();
        SetOrigin();
        init = true;
    }

    public List<Transform> getjoints(GameObject skeleton) {

        List<Transform> joints = new List<Transform>();

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

    public List<Quaternion?> getOrigins(List<Transform> list) {

        List<Quaternion?> listQ = new List<Quaternion?>();

        foreach (Transform t in list) {

            if (t == null)
                listQ.Add(null);
            else 
                listQ.Add(t.localRotation);
        }

        return listQ;
    }

    public void ApplyPose() {

       

        int i = 0;

        foreach (Transform s in SourceJoints) {

            if (TargetJoints[i] != null) { 

                Transform t = (Transform)TargetJoints[i];

                if (s != null && t!= null){

                    Debug.Log(" ----------------- ");
                    Debug.Log(s.name + " " + t.name);
                    Debug.Log(" ----------------- ");

                    Applica(s, t, (Quaternion)TargetOrigins[i]);

                }

            }

            i += 1;
        }

    }

    public void Back2Origin()
    {
        int i = 0;

        foreach (Transform t in TargetJoints)
        {
            if (t != null) { 
                t.localRotation = (Quaternion)TargetOrigins[i]; 
            }

            i += 1;

        }

        
    }

    public void SetOrigin()
    {
        if (Source != null) SourceOrigins = getOrigins(SourceJoints);
        if (Target != null) TargetOrigins = getOrigins(TargetJoints);
    }

    public void SetJoints() {
        if (Source != null) SourceJoints = getjoints(Source);
        if (Target != null)  TargetJoints = getjoints(Target);
    }

    private void Applica(Transform SourceJoint , Transform TargetJoint, Quaternion OriginalQ)
        {
            float angle = 0f;
            Vector3 axisLocalSource = Vector3.zero;
            SourceJoint.localRotation.ToAngleAxis(out angle, out axisLocalSource);

            Vector3 axisGlobal = SourceJoint.TransformVector(axisLocalSource);
            Debug.Log("angle-->" + angle.ToString());
            Debug.Log("source local axis-->" + axisLocalSource.ToString());
            Debug.Log("global axis-->" + axisGlobal.ToString());

            TargetJoint.localRotation = OriginalQ;
            Debug.Log("OriginalQ-->" + OriginalQ.ToString());
            Vector3 axisLocalTarget = TargetJoint.InverseTransformVector(axisGlobal);
            Debug.Log("target local axis-->" + axisLocalTarget.ToString());
            TargetJoint.Rotate(axisLocalTarget, angle);
        }

    public Quaternion ApplicaDuringSerialization(Transform SourceJoint) {


        int index = 0;

        for (index=0; index< TargetJoints.Count - 1; index+=1) {
            if (TargetJoints[index] == null)  continue;
            if (TargetJoints[index].name == d[SourceJoint.name]) break;
        }

        if (index == 0) {
            SourceJoint = SourceJoint.transform.parent;
            TargetJoints[index].transform.parent.transform.position = new Vector3(SourceJoint.position.x, 0f, SourceJoint.position.z);
        }

        float angle = 0f;
        Vector3 axisLocalSource = Vector3.zero;
        SourceJoint.localRotation.ToAngleAxis(out angle, out axisLocalSource);
        Vector3 axisGlobal = SourceJoint.TransformVector(axisLocalSource);


        TargetJoints[index].localRotation = (Quaternion)TargetOrigins[index];     
        Vector3 axisLocalTarget = TargetJoints[index].InverseTransformVector(axisGlobal);
        TargetJoints[index].Rotate(axisLocalTarget, angle);

        return TargetJoints[index].localRotation;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(pose_copier))]
    public class pose_copier_Editor : Editor
    {

        public pose_copier Target;

        void Awake()
        {
            Target = (pose_copier)target;
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

                Target.source = EditorGUILayout.ObjectField("SkeletonSource", Target.source, typeof(GameObject), true) as GameObject;

                Target.target = EditorGUILayout.ObjectField("SkeletonTarget", Target.target, typeof(GameObject), true) as GameObject;


                if (Utility.GUIButton("ApplyPose", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.ApplyPose();
                }


                if (Utility.GUIButton("Back2Origin", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Back2Origin();
                }

                if (Utility.GUIButton("SetOrigin", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.SetOrigin();
                }

                if (Utility.GUIButton("SetJoints", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.SetJoints();
                }

            }
        }
    }
#endif
}

