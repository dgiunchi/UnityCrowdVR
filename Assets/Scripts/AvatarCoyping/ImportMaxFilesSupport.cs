
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Autodesk.Fbx;
using System;

public class ImportMaxFilesSupport : MonoBehaviour
{
    [MenuItem("Assets/ImportMaxFileHelper/Change shaders of materials", true)]
    private static bool CreateScriptableObjAsAssetValidator()
    {
        var activeObject = Selection.activeObject;

        // make sure it is a text asset
        if ((activeObject == null) || !(activeObject is GameObject))
        {
            return false;
        }

        // make sure it is a persistant asset
        var assetPath = AssetDatabase.GetAssetPath(activeObject);
        if (assetPath == null)
        {
            return false;
        }

        // load the asset as a GameObject
        var gObject = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
        if (gObject == null)
        {
            return false;
        }


        return true;
    }

    [MenuItem("Assets/ImportMaxFileHelper/Change shaders of materials")]
    static void SaveMaterialsAndApplyToRenderers(MenuCommand command)
    {
        // we already validated this path, and know these calls are safe
        var activeObject = Selection.activeObject;
        var assetPath = AssetDatabase.GetAssetPath(activeObject);
        var path = assetPath.Replace(activeObject.name + "." + assetPath.Split('.')[1], "");      
        var go = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

        GameObject newgo = Instantiate(go);

        var skinnedMeshRenderer = newgo.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer smr in skinnedMeshRenderer) {

            Shader newshader = Shader.Find("Unlit/shaderAvatarDenoialtri");

            Material[] mlist = new Material[smr.sharedMaterials.Length];
            int i = 0;

            foreach (Material m in smr.sharedMaterials) {

                string pathMaterialasset = path + m.name + ".mat";

                if (System.IO.File.Exists(pathMaterialasset))
                {
                    mlist[i] =  (Material)AssetDatabase.LoadAssetAtPath(pathMaterialasset, typeof(Material));

                }
                else 
                {
                    mlist[i] = new Material(m);

                    mlist[i].shader = newshader;

                    AssetDatabase.CreateAsset(mlist[i], pathMaterialasset);
                }

                i += 1;

            }

            smr.sharedMaterials = mlist;

        }


        PrefabUtility.SaveAsPrefabAsset(newgo, path + activeObject.name + "_unlitShader.prefab");

        DestroyImmediate(newgo);

        AssetDatabase.DeleteAsset(assetPath);


    }


    [MenuItem("GameObject/ImportMaxFileHelper/ResizeAndUnpack", false, 10)]
    static void ResizeAndUnpack(MenuCommand menuCommand)
    {
        GameObject Avatar = Selection.activeTransform.gameObject;

        Avatar.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        PrefabUtility.UnpackPrefabInstance(Avatar, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
    }


    [MenuItem("GameObject/ImportMaxFileHelper/AddLODToFBXRocketAvatar", false, 10)]
    static void AddLODToRocketAvatar(MenuCommand menuCommand)
    {
        GameObject Avatar = Selection.activeTransform.gameObject;

        //get the levels 

        Transform[] children = Avatar.GetComponentsInChildren<Transform>(true);

        Renderer [] hipoly = null;
        Renderer [] midpoly = null;
        Renderer [] lowpoly = null;
        Renderer [] ultralowpoly = null;

        foreach (Transform t in children) {

            if (t.name.Contains("hipoly"))
            {
                hipoly = t.gameObject.GetComponents<SkinnedMeshRenderer>();
                t.gameObject.SetActive(true);
            }
            else if (t.name.Contains("midpoly"))
            {
                midpoly = t.gameObject.GetComponents<SkinnedMeshRenderer>();
                t.gameObject.SetActive(true);
            }
            else if (t.name.Contains("ultralowpoly"))
            {
                ultralowpoly = t.gameObject.GetComponents<SkinnedMeshRenderer>();
                t.gameObject.SetActive(true);
            }
            else if (t.name.Contains("lowpoly"))
            {
                lowpoly = t.gameObject.GetComponents<SkinnedMeshRenderer>();
                t.gameObject.SetActive(true);
            }

        }

        if (hipoly == null)
        {
            Debug.Log("hipoly missing I quit");
            return;
        }
        else if (midpoly==null)
        {
            Debug.Log("midpoly missing I quit");
            return;
        }
        else if (lowpoly == null)
        {
            Debug.Log("lowpoly missing I quit");
            return;
        }
        else if (ultralowpoly == null)
        {
            Debug.Log("ultralowpoly missing I quit");
            return;
        }


        // Add the  LOD 
        LODGroup group = Avatar.AddComponent<LODGroup>();


        // Add 4 LOD levels
        LOD[] lods = new LOD[5];
        lods[0] = new LOD(0.85f, hipoly);
        lods[1] = new LOD(0.65f, midpoly);
        lods[2] = new LOD(0.40f, lowpoly);
        lods[3] = new LOD(0.01f, ultralowpoly);

        group.SetLODs(lods);
        group.RecalculateBounds();

    }

 
}
#endif 