﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    //public 
    public string Lobby;

    public string End;

    public ScriptableObject experiment;

    public UIBuilder uimanager;

    public GameObject[] DontDelete;

    //private 
    private ExperimentScriptableObject e;

    private bool started;

    private Queue<ExperimentAction> ActionQueue = new Queue<ExperimentAction>();

    private bool Executing = false;

    void Awake()
    {
        e = (ExperimentScriptableObject)experiment;

        foreach (GameObject g in DontDelete) DontDestroyOnLoad(g);

        //find end of trialobject and link to OnEndOfTrialEvent
        GameObject.Find("EndOfTrialManager").GetComponent<EndOfTrialManager>().OnEndOfTrialEvent.AddListener(NextAction);
    }

    private void Start()
    {
        StartExperiment();
    }

    public void StartExperiment() {

        if (started) return;
        else started = true;

        createQueueTrials();

    }

    void createQueueTrials() {

        foreach (Session s in e.Session)
        {

            if (s.randomizeTrialsOrder)
            {
                ActionQueue = RandomizeTrialOrder(s);//here we can have also another method based on partecipant numbers instead
            }
            else
            {
                foreach (Trial t in s.trials)
                {
                    ExperimentAction scene = new ExperimentAction(ExperimentActionType.scene,t.sceneName);
                    ActionQueue.Enqueue(scene);
                    
                    ExperimentAction questionaire = new ExperimentAction(ExperimentActionType.QuestoinairePart, t.questionaireRefName);
                    ActionQueue.Enqueue(questionaire);
                }
            }
        }

    }

    Queue<ExperimentAction> RandomizeTrialOrder(Session s) {
        
        Queue<ExperimentAction> q = new Queue<ExperimentAction>();

        List<int> array = new List<int>();

        while (array.Count< s.trials.Length) {

            int newindex = Random.Range(0, s.trials.Length);

            if (array.IndexOf(newindex) == -1)
            {
                array.Add(newindex);
            }
          
           
        }

        foreach (int i in array) {

            ExperimentAction scene = new ExperimentAction(ExperimentActionType.scene, s.trials[i].sceneName);
            q.Enqueue(scene);

            ExperimentAction questionaire = new ExperimentAction(ExperimentActionType.QuestoinairePart, s.trials[i].questionaireRefName);
            q.Enqueue(questionaire);
        }


        return q;

    }

    public void NextAction() {

        if (Executing)
        {
            return;
        }
        else
        {
            Executing = true;
            if (ActionQueue.Count > 0)
            {
                StartCoroutine(ExecuteAction(ActionQueue.Dequeue()));
            }
            else
            {
                StartCoroutine(LoadYourAsyncScene(End));
            }
            
        }

    }

    IEnumerator ExecuteAction(ExperimentAction a)
    {
        if (a.type == ExperimentActionType.QuestoinairePart)
        {
            //load Lobby scene 
            yield return LoadYourAsyncScene(Lobby);

            //Build Questionaire UI
            uimanager.Build(a.ActionName);

            //add listener to move to next action 
            uimanager.OnQuestionairePartCompleted.AddListener(NextAction);
            
        }
        else if (a.type == ExperimentActionType.scene)
        {
            yield return  LoadYourAsyncScene(a.ActionName);

            //find end of trialobject and link to OnEndOfTrialEvent
            //GameObject.Find("[REPLACEWITHCORRECTNAME]").GetComponent<PositionSerializerAdam>().OnEndOfTrialEvent.AddListener(NextAction);
            GameObject.Find("EndOfTrialManager").GetComponent<EndOfTrialManager>().OnEndOfTrialEvent.AddListener(NextAction);
            
        }
        
        Executing = false;
    }

    IEnumerator LoadYourAsyncScene(string scenename)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        TransitionManager.Instance.setWaitingView();
        yield return new WaitForSeconds(1.0f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenename);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void LoadScene(string scenename)
    {
        Debug.Log("sceneName to load: " + scenename);
        SceneManager.LoadScene(scenename);
    }


}

public class ExperimentAction {

    public ExperimentActionType type;
    public string ActionName;

    public ExperimentAction(ExperimentActionType scene, string sceneName)
    {
        this.type = scene;
        this.ActionName = sceneName;
    }
}


public enum ExperimentActionType { 

    QuestoinairePart,
    scene,
}