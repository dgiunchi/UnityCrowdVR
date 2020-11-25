using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UnityEngine.UI;

using SpaceBear.VRUI;
using UnityEngine.EventSystems;

public class UIBuilder : MonoBehaviour
{
    
    public ScriptableObject questionaire;

    public ScriptableObject useriinterface;

    protected GameObject Panel;

    protected QuestionaireUI ui;

    protected Questionaire q;

    public Firebase databaseManager;

    void onAwake() {

        ui = (QuestionaireUI)useriinterface;
        q = (Questionaire)questionaire;

        cleanQuestionaire();
    }

    void cleanQuestionaire() {


        foreach (QuestionairePart p in q.questionaire.parts) {

            foreach (QuestionaireQuestion q in p.questions) {

                q.answer = null;

            }
        }
    }
    void Build(int part)
    {

#if UNITY_EDITOR
        ui = (QuestionaireUI)useriinterface;
        q = (Questionaire)questionaire;

        cleanQuestionaire();
#endif 

        Panel = new GameObject("Panel");

        GameObject Container = Instantiate(ui.Container, Panel.transform);
        GameObject PanelTitle = Instantiate(ui.PanelTitle, Container.transform);
        PanelTitle.GetComponent<Text>().text = q.questionaire.parts[part].name;

        for (int questionNumber = 0; questionNumber < q.questionaire.parts[part].questions.Length; questionNumber++)
        {

            QuestionaireQuestion question = q.questionaire.parts[part].questions[questionNumber];

            GameObject QuestionTitle = Instantiate(ui.Title, Container.transform);
            QuestionTitle.GetComponent<Text>().text = question.question;
            GameObject QuestionText = Instantiate(ui.Text, Container.transform);
            QuestionText.GetComponent<Text>().text = question.helpText;

            if (question.uielement == UitType.Radio)
            {

                GameObject QuestionUiElement = Instantiate(ui.Radio, Container.transform);
                QuestionUiElement.name = "Q"+part.ToString() + questionNumber.ToString();

                Transform ChildOption = QuestionUiElement.transform.GetChild(0);
                int l = QuestionUiElement.transform.GetChildCount();
                for (int i = 1; i < l; i++)
                {
                    Transform QuestionUiElementChild = QuestionUiElement.transform.GetChild(QuestionUiElement.transform.GetChildCount() - 1);

#if UNITY_EDITOR
                    DestroyImmediate(QuestionUiElementChild.gameObject);
#else
                    Destroy(QuestionUiElementChild.gameObject);
#endif 
                }


                for (int i = 0; i < question.Options.Length; i++)
                {

                    //generate captured variables for lambda functions
                    var newPart = part;
                    var newQuestionNumber = questionNumber;
                    var newOptionNumber = i;
                    GameObject Option;

                    if (i == 0)  Option = ChildOption.gameObject;
                    else Option = Instantiate(ChildOption.gameObject, QuestionUiElement.transform);                       
                    
                    Option.GetComponentInChildren<Text>().text = question.Options[i];
                    VRUIRadio vruiradio = Option.GetComponentInChildren<VRUIRadio>();
                    vruiradio.onPointerClick.AddListener(delegate { RadioValueChanged(newPart, newQuestionNumber, newOptionNumber); });
                   
                }

                foreach (VRUIRadio vruiradio in QuestionUiElement.GetComponentsInChildren<VRUIRadio>())
                {
                     vruiradio.isOn = false;
                }

            }
            else if (question.uielement == UitType.Slider)
            {
                //generate captured variables for lambda functions
                var newPart = part;
                var newQuestionNumber = questionNumber;

                GameObject QuestionUiElement = Instantiate(ui.Slider, Container.transform);
                QuestionUiElement.name = "Q" + newPart.ToString() + newQuestionNumber.ToString();
                QuestionUiElement.GetComponentInChildren<VRUISlider>().onValueChanged.AddListener((value) => SliderValueChanged(newPart, newQuestionNumber, value));
            }

        }

        GameObject Button = Instantiate(ui.Button, Container.transform);
        Button.GetComponentInChildren<Button>().onClick.AddListener(delegate { SaveQuestionaire(part); });
    }

    public void RadioValueChanged(int part,int questionNumber, int indexOption)
    {
        string value = q.questionaire.parts[part].questions[questionNumber].Options[indexOption];
        Debug.Log("Chaging Questionaire Part " + part.ToString() + " Question->" + questionNumber.ToString()+" Value->"+ value);
        q.questionaire.parts[part].questions[questionNumber].answer = value;
        deleteNotifications(part, questionNumber);
    }

    public void SliderValueChanged(int part, int questionNumber, float value)
    {
        Debug.Log("Chaging Questionaire Part "+ part.ToString() + " Question->" + questionNumber.ToString() + " Value->" + value.ToString());
        q.questionaire.parts[part].questions[questionNumber].answer = value.ToString();
        deleteNotifications(part, questionNumber);
    }

    public void SaveQuestionaire(int part) {

        if (!triggerNotifactions(part)) { 

                databaseManager.SaveQuestionaire(q);
                Destroy();
        }
   
    }

    public bool triggerNotifactions(int part) {
       
        List<int> AnansweredAnswers = checkAnansweredAnswers(part);

        if (AnansweredAnswers.Count == 0) return false;
        else
        {
            GenerateNotification(part, AnansweredAnswers);
            return true;
        }

    }

    List<int> checkAnansweredAnswers(int part)
    {
        List<int> anaswered = new List<int>();

        for (int i=0; i < q.questionaire.parts[part].questions.Length;i++)
        {
            QuestionaireQuestion question = q.questionaire.parts[part].questions[i];

            if (question.answer == null)
            {
                anaswered.Add(i);
            }
        }

        return anaswered;
    }

    void GenerateNotification(int part, List<int> AnansweredAnswers) {

        deleteNotifications();

        foreach(int index in AnansweredAnswers){

            Transform AnasweredQ = Panel.transform.FindDeepChild("Q"+part+index);

            GameObject allert = Instantiate(ui.Notifications);

            allert.transform.position = AnasweredQ.position;

            allert.name = "Notification"+part + index;

            allert.transform.Translate(new Vector3(2.50f, 0f,0f));

        }
    
    }

    void deleteNotifications() {

        GameObject[] Notifications = GameObject.FindGameObjectsWithTag("Notification");

        foreach (GameObject n in Notifications)  Destroy(n);
        
    }


    void deleteNotifications(int part, int questionNumber)
    {

        GameObject Notification = GameObject.Find("Notification" + part + questionNumber);

        Destroy(Notification);

    }

    void Destroy()
    {

#if UNITY_EDITOR
        DestroyImmediate(Panel);
#else

    Destroy(Panel);
#endif 

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UIBuilder))]
	public class UIBuilder_Editor : Editor
	{

		public UIBuilder Target;

		void Awake()
		{
			Target = (UIBuilder)target;
		}

		public override void OnInspectorGUI()
		{


			Undo.RecordObject(Target, Target.name);

			Inspector();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(Target);
			}
		}

        private void Inspector()
        {
            Utility.SetGUIColor(UltiDraw.Grey);
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                Utility.ResetGUIColor();

                EditorGUILayout.LabelField("UI builder");

                Target.questionaire = EditorGUILayout.ObjectField("questionaire", Target.questionaire, typeof(ScriptableObject), true) as ScriptableObject;
                Target.useriinterface = EditorGUILayout.ObjectField("ui", Target.useriinterface, typeof(ScriptableObject), true) as ScriptableObject;
                Target.databaseManager = EditorGUILayout.ObjectField("databaseManager", Target.databaseManager, typeof(Firebase), true) as Firebase;

                if (Utility.GUIButton("Layout Create Test", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Build(0);
                }
                if (Utility.GUIButton("Destroy Test", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Destroy();
                }




            }
        }
    }
#endif


}

