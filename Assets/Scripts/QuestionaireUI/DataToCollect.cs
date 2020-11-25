using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Questionaire/NewQuestionaire")]
public class DataToCollect : ScriptableObject
{
    [SerializeField]
    public Questionaire questionaire;

}

[Serializable]
public class Questionaire
{
    public QuestionairePart[] parts;
}

[Serializable]
public class QuestionairePart
{
    public string name;
    public string referenceName;
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
public class DataToCollectUpload : ScriptableObject
{
    [SerializeField]
    public QuestionaireUpload questionaire;

    public DataToCollectUpload(Questionaire v)
    {
        this.questionaire = (QuestionaireUpload)v;
    }

    public static explicit operator DataToCollectUpload(DataToCollect v)
    {
        return new DataToCollectUpload(v.questionaire);
    }

}

[Serializable]
public class QuestionaireUpload
{
    public QuestionairePartUpload[] parts;

    public QuestionaireUpload(QuestionairePart[] v)
    {
        this.parts = new QuestionairePartUpload[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            this.parts[i] = (QuestionairePartUpload)v[i];
        }
    }

    public static explicit operator QuestionaireUpload(Questionaire v)
    {
        return new QuestionaireUpload(v.parts);
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
        return new QuestionairePartUpload(v.referenceName, v.description, v.questions);
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
