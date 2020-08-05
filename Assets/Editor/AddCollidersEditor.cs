using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AddCollidersEditor : ScriptableObject
{

    [MenuItem("Custom/Collider add scripts")]
    static void AddColliderScripts()
    {

        var allObjects = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer m in allObjects)
        {
            GameObject obj = m.gameObject;
            Collider ds = obj.GetComponent(typeof(Collider)) as Collider;
            if (!ds)
                obj.AddComponent(typeof(MeshCollider));

        }
    }

}

/*public class ProcessAll : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        var objects = GameObject.FindObjectsOfType<MeshRenderer>();
        var objectCount = objects.Length;
        foreach (var obj in objects)
        {
            GameObject o = obj.gameObject;
            Collider c = o.GetComponent<Collider>();
            if (c == null)
            {
                MeshCollider m = o.AddComponent<MeshCollider>();
                if (o.GetComponent<MeshCollider>() == null)
                {
                    Debug.Log("No Mesh collider :\\ " + o.name);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}*/
