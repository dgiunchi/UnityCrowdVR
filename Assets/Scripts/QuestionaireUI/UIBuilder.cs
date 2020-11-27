using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UnityEngine.UI;

using SpaceBear.VRUI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIBuilder : MonoBehaviour
{

    public GameObject Parent;

    public float scale;

    public Vector3 adjust;

    public ScriptableObject dataToCollect;

    public ScriptableObject useriinterface;

    protected GameObject Panel;

    protected QuestionaireUI ui;

    protected DataToCollect data;

    public Firebase databaseManager;

    public UnityEvent OnQuestionairePartCompleted;


    void Awake() {

        ui = (QuestionaireUI)useriinterface;
        data = (DataToCollect)dataToCollect;

        cleanQuestionaire();
    }

    void cleanQuestionaire() {


        foreach (QuestionairePart p in data.questionaire.parts) {

            foreach (QuestionaireQuestion q in p.questions) {

                q.answer = null;

            }
        }
    }

    public void EditorBuild(int i) { 

#if UNITY_EDITOR

        ui = (QuestionaireUI)useriinterface;
        data = (DataToCollect)dataToCollect;

        cleanQuestionaire();

#endif

        string referenceName = data.questionaire.parts[i].referenceName;

        Build(referenceName);
    } 

    public void Build(string referenceName)
    {

        int part = GetQuestionairePartIndex(referenceName);

        if (part == -1) {

            Debug.Log("Questionaire with Refrence name -->" + referenceName+" not found can't build interface");
            return;
        }

        Panel = new GameObject("Questionaire Panel");

        if (Parent != null) {

            Panel.transform.position = Parent.transform.position;
            Panel.transform.rotation = Parent.transform.rotation;
            Panel.transform.localScale = new Vector3(scale, scale, scale);
            Panel.transform.Translate(adjust);
        }

        GameObject Container = Instantiate(ui.Container, Panel.transform);
        GameObject PanelTitle = Instantiate(ui.PanelTitle, Container.transform);
        PanelTitle.GetComponent<Text>().text = data.questionaire.parts[part].name;

        for (int questionNumber = 0; questionNumber < data.questionaire.parts[part].questions.Length; questionNumber++)
        {

            QuestionaireQuestion question = data.questionaire.parts[part].questions[questionNumber];

            GameObject QuestionTitle = Instantiate(ui.Title, Container.transform);
            QuestionTitle.GetComponent<Text>().text = question.question;
            GameObject QuestionText = Instantiate(ui.Text, Container.transform);
            QuestionText.GetComponent<Text>().text = question.helpText;

            if (question.uielement == UitType.Radio)
            {

                GameObject QuestionUiElement = Instantiate(ui.Radio, Container.transform);
                QuestionUiElement.name = "Q"+part.ToString() + questionNumber.ToString();

                Transform ChildOption = QuestionUiElement.transform.GetChild(0);
                int l = QuestionUiElement.transform.childCount;
                for (int i = 1; i < l; i++)
                {
                    Transform QuestionUiElementChild = QuestionUiElement.transform.GetChild(QuestionUiElement.transform.childCount - 1);

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

    int GetQuestionairePartIndex(string referenceName) {


        for (int i=0; i<data.questionaire.parts.Length; i++)
        {

            if (referenceName == data.questionaire.parts[i].referenceName)
            { 
                return i; 
            }

        }

        return -1;
    }

    public void RadioValueChanged(int part,int questionNumber, int indexOption)
    {
        string value = data.questionaire.parts[part].questions[questionNumber].Options[indexOption];
        Debug.Log("Chaging Questionaire Part " + part.ToString() + " Question->" + questionNumber.ToString()+" Value->"+ value);
        data.questionaire.parts[part].questions[questionNumber].answer = value;
        deleteNotifications(part, questionNumber);
    }

    public void SliderValueChanged(int part, int questionNumber, float value)
    {
        Debug.Log("Chaging Questionaire Part "+ part.ToString() + " Question->" + questionNumber.ToString() + " Value->" + value.ToString());
        data.questionaire.parts[part].questions[questionNumber].answer = value.ToString();
        deleteNotifications(part, questionNumber);
    }

    public void SaveQuestionaire(int part) {

        if (!triggerNotifactions(part)) { 

            databaseManager.SaveQuestionaire(data);
            Destroy();

            OnQuestionairePartCompleted.Invoke();

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

        for (int i=0; i < data.questionaire.parts[part].questions.Length;i++)
        {
            QuestionaireQuestion question = data.questionaire.parts[part].questions[i];

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
            allert.name = "Notification" + part + index;

            if (Parent != null)
            {
                allert.transform.localScale = new Vector3(scale, scale, scale);
                allert.transform.localPosition = AnasweredQ.position;
                allert.transform.rotation = AnasweredQ.rotation;
                allert.transform.Translate(new Vector3(2.50f*scale, 0f, 0f), Space.Self);
            }
            else { 
                allert.transform.position = AnasweredQ.position;    
                allert.transform.Translate(new Vector3(2.50f, 0f,0f));                 
            }
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

                Target.adjust = EditorGUILayout.Vector3Field("Adjust", Target.adjust);
                Target.scale = EditorGUILayout.FloatField("Scale", Target.scale);
                Target.Parent = EditorGUILayout.ObjectField("Parent", Target.Parent, typeof(GameObject), true) as GameObject;
                Target.dataToCollect = EditorGUILayout.ObjectField("Data To Collect", Target.dataToCollect, typeof(ScriptableObject), true) as ScriptableObject;
                Target.useriinterface = EditorGUILayout.ObjectField("Ui Prefabs", Target.useriinterface, typeof(ScriptableObject), true) as ScriptableObject;
                Target.databaseManager = EditorGUILayout.ObjectField("Database Manager", Target.databaseManager, typeof(Firebase), true) as Firebase;

                if (Utility.GUIButton("Layout Create Test", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.EditorBuild(0);
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

