using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Questionaire/NewDataToCollectTest")]
public class DataToCollectTest : ScriptableObject
{
    [SerializeField]
    public QuestionaireTest questionaire;

}

[Serializable]
public class QuestionaireTest
{
    [SerializeField]
    public QuestionairePartTest[] parts;
}


[Serializable]
public class QuestionairePartTest
{
    [SerializeField]
    public string name;
    [SerializeField]
    public string referenceName;
    [SerializeField]
    public string description;
    [SerializeField]
    private QuestionaireSubPartTest[] subparts;
    
    [SerializeField]
    public ScriptableObject[] subpartsScripts {

        set {

            subparts = new QuestionaireSubPartTest[value.Length];

            for (int i=0; i<value.Length; i++) { 
                subparts[i] = (QuestionaireSubPartTest)value[i];
            }
            
        }

    }

}
