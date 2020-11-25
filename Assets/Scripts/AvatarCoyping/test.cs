using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject Source;

    public Quaternion OriginalQ;

    private void Applica()
    {       
        float angle = 0f;
        Vector3 axisLocalSource = Vector3.zero;
        Source.transform.localRotation.ToAngleAxis(out angle,out axisLocalSource);
       
        Vector3 axisGlobal = Source.transform.TransformVector(axisLocalSource);
        Debug.Log("angle-->" + angle.ToString());
        Debug.Log("source local axis-->" + axisLocalSource.ToString());
        Debug.Log("global axis-->"+axisGlobal.ToString());

        gameObject.transform.localRotation = OriginalQ;
        Debug.Log("OriginalQ-->" + OriginalQ.ToString());
        Vector3 axisLocalTarget = transform.InverseTransformVector(axisGlobal);
        Debug.Log("target local axis-->" + axisLocalTarget.ToString());
        gameObject.transform.Rotate(axisLocalTarget, angle);
    }

    private void Original()
    {
        
        gameObject.transform.localRotation = OriginalQ;

    }

    private void setOriginal()
    {

        OriginalQ = gameObject.transform.localRotation;

    }



#if UNITY_EDITOR
    [CustomEditor(typeof(test))]
public class test_Editor : Editor
{

    public test Target;



    void Awake()
    {
        Target = (test)target;
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

            Target.Source = EditorGUILayout.ObjectField("SkeletonRecord", Target.Source, typeof(GameObject), true) as GameObject;

         
            if (Utility.GUIButton("Applica", UltiDraw.DarkGrey, UltiDraw.White))
            {
                    Target.Applica();
            }

            if (Utility.GUIButton("Original", UltiDraw.DarkGrey, UltiDraw.White))
            {
                Target.Original();
            }

            if (Utility.GUIButton("SetOriginal", UltiDraw.DarkGrey, UltiDraw.White))
            {
                Target.setOriginal();
            }


            }
    }
}
#endif
}

