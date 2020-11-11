using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class count : MonoBehaviour
{
 
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.gameObject.transform.GetChildCount());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
