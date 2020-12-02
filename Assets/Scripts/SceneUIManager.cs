using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneUIManager : MonoBehaviour
{
    public GameObject[] listui;
   
    int i=0;

    public void toggleUI() {

        listui[i].SetActive(!listui[i].activeSelf);

        if (!listui[i].activeSelf) i += 1;
        
    }

}
