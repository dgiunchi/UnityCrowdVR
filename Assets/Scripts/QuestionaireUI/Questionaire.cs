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
    public string answer;
}

public enum UitType
{
    Slider,
    Radio,
}


//upload dataset
public class QuestionaireUpload : ScriptableObject
{
    [SerializeField]
    public QuestionaireObjectUpload questionaire;

    public QuestionaireUpload(QuestionaireObject v)
    {
        this.questionaire = (QuestionaireObjectUpload)v;
    }

    public static explicit operator QuestionaireUpload(Questionaire v)
    {
        return new QuestionaireUpload(v.questionaire);
    }

}

[Serializable]
public class QuestionaireObjectUpload
{
    public QuestionairePartUpload[] parts;

    public QuestionaireObjectUpload(QuestionairePart[] v)
    {
        this.parts = new QuestionairePartUpload[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            this.parts[i] = (QuestionairePartUpload)v[i];
        }
    }

    public static explicit operator QuestionaireObjectUpload(QuestionaireObject v)
    {
        return new QuestionaireObjectUpload(v.parts);
    }
}

[Serializable]
public class QuestionairePartUpload
{
    public string name;
    public string description;
    public QuestionaireQuestionUpload[] questions;


    public QuestionairePartUpload(string name, string description, QuestionaireQuestion[] questions1)
    {
        this.name = name;
        this.description = description;

        this.questions = new QuestionaireQuestionUpload[questions1.Length];

        for (int i= 0; i < questions.Length; i++)
        {
            this.questions[i] = (QuestionaireQuestionUpload)questions1[i];
        }

    }

    public static explicit operator QuestionairePartUpload(QuestionairePart v)
    {
        return new QuestionairePartUpload(v.name, v.description, v.questions);
    }
}

[Serializable]
public class QuestionaireQuestionUpload
{
    public string question;
    public string answer;

    public QuestionaireQuestionUpload(string question, string answer)
    {
        this.question = question;
        this.answer = answer;
    }


    public static explicit operator QuestionaireQuestionUpload(QuestionaireQuestion v)
    {
        return new QuestionaireQuestionUpload(v.question,v.answer);
    }
}
