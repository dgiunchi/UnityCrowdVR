using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour { //@@TODO thisclass collect ALL the interaction.... so manage this to make it works

	private static List<HashSet<KeyCode>> Keys = new List<HashSet<KeyCode>>();
	private static int Capacity = 2;
	private static int Clients = 0;

	void OnEnable() {
		Clients += 1;
	}

	void OnDisable() {
		Clients -= 1;
	}

	public static bool anyKey {
		get{
			for(int i=0; i<Keys.Count; i++) {
				if(Keys[i].Count > 0) {
					return true;
				}
			}
			return false;
		}
	}

	void Update () {        
        while (Keys.Count >= Capacity) {
			Keys.RemoveAt(0);
		}

        HashSet<KeyCode> state = new HashSet<KeyCode>();
#if !UNITY_ANDROID || UNITY_EDITOR
		foreach(KeyCode k in Enum.GetValues(typeof(KeyCode))) {
			if(Input.GetKey(k)) {
                /*if(k == KeyCode.Mouse0) // intercpt, for the mouse, for the Oculus triggr should b the same
                {
                    state.Add(KeyCode.W);
                } else if (k == KeyCode.Mouse1)
                {
                    state.Add(KeyCode.S);
                }
                else
                {
                    state.Add(k);
                }*/
                state.Add(k);               

            }
		}
		Keys.Add(state);
#else 
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0)
        {
            state.Add(KeyCode.D);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < 0)
        {
            state.Add(KeyCode.A);
        }


        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y > 0)
        {
            state.Add(KeyCode.W);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y < 0)
        {
            state.Add(KeyCode.S);
        }

        if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x > 0) //|| OVRInput.Get(OVRInput.Button.Three)
        {
            state.Add(KeyCode.E);
        }
        else if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x < 0) //|| OVRInput.Get(OVRInput.Button.Four)
        {
            state.Add(KeyCode.Q);
        }

         //Debug.Log("From  Debug:" + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x.ToString());

        Keys.Add(state);
#endif //!UNITY_ANDROID 
    }

    public static bool GetKey(KeyCode k) {
		if(Clients == 0) {
			return Input.GetKey(k);
		} else {
			for(int i=0; i<Keys.Count; i++) {
				if(Keys[i].Contains(k)) {
					return true;
				}
			}
			return false;
		}
	}
	
}
