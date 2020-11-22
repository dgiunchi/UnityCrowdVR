using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class PlayRunTime : MonoBehaviour
{
    bool triggerPlay = false;
    private VideoPlayer MyVideoPlayer;

    public void PlayVideo()
    {
        MyVideoPlayer = GetComponent<VideoPlayer>();
        // play video player
        MyVideoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetKeyDown(KeyCode.P) || OVRInput.Get(OVRInput.Button.Three) && triggerPlay == false)
        {
            PlayVideo();
            triggerPlay = true;
        }
    }
}
