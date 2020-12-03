using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterArriving : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "Player")
        {
            // experiment ended here
            // @@todo change scene, or do other
            StartCoroutine(LoadNextScene());
        }
    }

    IEnumerator LoadNextScene()
    {
        
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);

        SceneManager.LoadScene("sample_network_scene_end", LoadSceneMode.Single);
        
    }
}
