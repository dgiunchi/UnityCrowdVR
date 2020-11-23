using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Questionaire/NewQuestionaire")]
public class Questionaire : ScriptableObject
{
    [SerializeField]
    public QuestionaireObject questionaire;

}

[Serializable]
public class QuestionaireObject
{
    public QuestionairePart[] parts;
}

[Serializable]
public class QuestionairePart
{
    public string name;
    public string description;
    public QuestionaireQuestionSet QuestionSet;

}

[Serializable]
public class QuestionaireQuestionSet
{
    public string description;
    public QuestionaireQuestion[] questions;
}


[Serializable]
public class QuestionaireQuestion
{
    public string question;
    public string helpText;
    public UitType uielement;
    public bool valuebool;
    public float valuefloat;
    public string[] Options;
}




public enum UitType
{
    Slider,
    Radio,
}


