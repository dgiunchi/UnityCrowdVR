using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugByGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        GUI.color = Color.blue;
        UnityEditor.Handles.color = Color.black;
        string skeletonName = gameObject.name;
        UnityEditor.Handles.Label(new Vector3(transform.position.x, 1.0f, transform.position.z), gameObject.name);
    }
}
