using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Questionaire/NewQuestionaireSubPartTest")]
public class QuestionaireSubPartTest : ScriptableObject
{
    public string name;
    public string description;
    public QuestionaireQuestionTest[] questions;


}

[Serializable]
public class QuestionaireQuestionTest
{
    public string question;
    public string helpText;
    public UitType uielement;
    public bool valuebool;
    public float valuefloat;
    public string[] Options;
    public string answer;
}