using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UnityEngine.UI;


public class UIBuilder : MonoBehaviour
{
    
    public ScriptableObject questionaire;

    public ScriptableObject useriinterface;

    protected GameObject Panel;

    protected QuestionaireUI ui;

    protected Questionaire q;

    public delegate void EndQuestionaire();
    public static event EndQuestionaire Ended;

    void Build() {

        ui = (QuestionaireUI)useriinterface;
        q = (Questionaire)questionaire;

        Panel = new GameObject("Panel");

        GameObject Container = Instantiate(ui.Container, Panel.transform);
        GameObject PanelTitle = Instantiate(ui.PanelTitle, Container.transform);
        PanelTitle.GetComponent<Text>().text =  q.questionaire.parts[0].name;

        foreach (QuestionaireQuestion question in q.questionaire.parts[0].QuestionSet.questions) { 

            GameObject QuestionTitle = Instantiate(ui.Title, Container.transform);
            QuestionTitle.GetComponent<Text>().text = question.question;
            GameObject QuestionText = Instantiate(ui.Text, Container.transform);
            QuestionText.GetComponent<Text>().text = question.helpText;

            if (question.uielement == UitType.Radio) {

                GameObject QuestionUiElement = Instantiate(ui.Radio, Container.transform);
                Transform ChildOption = QuestionUiElement.transform.GetChild(0);
                int l = QuestionUiElement.transform.GetChildCount();
                for (int i=1; i<l ;i++)
                {
                    Transform QuestionUiElementChild = QuestionUiElement.transform.GetChild(QuestionUiElement.transform.GetChildCount()-1);

#if UNITY_EDITOR
                    DestroyImmediate(QuestionUiElementChild.gameObject);
#else
                    Destroy(QuestionUiElementChild.gameObject);
#endif 
                }

                ChildOption.GetComponentInChildren<Text>().text = question.Options[0];

                for (int i = 1; i < question.Options.Length; i++)  {

                    GameObject Option = Instantiate(ChildOption.gameObject, QuestionUiElement.transform);
                    Option.GetComponentInChildren<Text>().text = question.Options[i];
                }

            }
            else if (question.uielement == UitType.Slider)
            {

                GameObject QuestionUiElement = Instantiate(ui.Slider, Container.transform);

            }
        }

        GameObject Button = Instantiate(ui.Button, Container.transform);
        

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

                if (Utility.GUIButton("Create Panel", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Build();
                }
                if (Utility.GUIButton("Destroy Panel", UltiDraw.DarkGrey, UltiDraw.White))
                {
                    Target.Destroy();
                }




            }
        }
    }
#endif


}

