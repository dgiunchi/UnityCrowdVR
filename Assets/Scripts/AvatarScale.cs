using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarScale : MonoBehaviour
{
    bool scaled = false;
    bool scalingUp = false;
    bool scalingDown = false;
    bool firstRound = true;

    public float speed = 1F;
    private float startTime;
    private float journeyLength;


    // Update is called once per frame
    void Update()
    {

#if UNITY_ANDROID
        if (OVRInput.Get(OVRInput.Button.Four)) 
        {
            if (scaled && (!scalingUp && !scalingDown)) scalingDown = true;
            else if (!scaled && (!scalingUp && !scalingDown)) scalingUp = true;

        }

        if (scalingUp) scaleUp();
        else if (scalingDown) scaledown();
#endif



    }

    void scaleUp() 
    {
        //intialize
        if (firstRound) {
            startTime = Time.time;
            journeyLength = Vector3.Distance(new Vector3(10, 10, 10), new Vector3(1, 1, 1));
            firstRound = false;
        }

        //scale
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        gameObject.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(10, 10, 10), fractionOfJourney);

        //cleanup
        if (gameObject.transform.localScale == new Vector3(10, 10, 10)) {
            firstRound = true;
            scalingUp = false; 
            scaled = true;
        }

        
    }

    void scaledown()
    {
        //intialize
        if (firstRound)
        {
            startTime = Time.time;
            journeyLength = Vector3.Distance(new Vector3(10, 10, 10), new Vector3(1, 1, 1));
            firstRound = false;
        }

        //scale
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        gameObject.transform.localScale = Vector3.Lerp(new Vector3(10, 10, 10), new Vector3(1, 1, 1), fractionOfJourney);

        //cleanup
        if (gameObject.transform.localScale == new Vector3(1, 1, 1))
        {
            firstRound = true;
            scalingDown = false;
            scaled = false;
        }
    }
}
