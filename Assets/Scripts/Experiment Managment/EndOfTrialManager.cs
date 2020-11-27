using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndOfTrialManager : MonoBehaviour
{
    public UnityEvent OnEndOfTrialEvent;

    public bool invoked = false;


    public void InvokeEndOfTrial() {

        if (invoked)
        {
            return;
        }
        else
        {
            TransitionManager.Instance.setWaitingView();
            invoked = false;
            OnEndOfTrialEvent.Invoke();
        }
    }
}
