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


    public void initialize()
    {
        Target = gameObject.transform.FindDeepChild("Proxy_avatar_rocket_box").gameObject;       
        SetJoints();
        SetOrigin();
        init = true;
    }

    public List<Transform> getjoints(GameObject skeleton) {

        List<Transform> joints = PositionSerializerAdam.getJoints(skeleton);

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
            if (TargetJoints[index].name == PositionSerializerAdam.d[SourceJoint.name]) break;
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

