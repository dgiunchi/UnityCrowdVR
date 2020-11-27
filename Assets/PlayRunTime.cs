using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
public class PlayRunTime : MonoBehaviour
{
    bool triggerPlay = false;
    private VideoPlayer videoPlayer;
    public UnityEvent OnEndOfTrialEvent;

    private void Start()
    {
        //TransitionManager.Instance.setExperimentView();
        videoPlayer = GetComponent<VideoPlayer>();
        //videoPlayer.loopPointReached += EndReached;
    }

    public void PlayVideo()
    {
        
        // play video player
        videoPlayer.Play();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        OnEndOfTrialEvent.Invoke();
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
