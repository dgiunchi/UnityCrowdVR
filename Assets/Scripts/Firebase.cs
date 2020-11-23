using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Firebase : MonoBehaviour
{

    public string apiKey = "AIzaSyDzrVNvuBRWkMcxh2xF4cbQWYaYIAG6BHo";

    private string identitytoolkitUri = "https://identitytoolkit.googleapis.com/";

    private string anonymousAuthentication = "/v1/accounts:signUp?key=";

    private string realTimeDatabase = "https://crowdvr.firebaseio.com/users/";

    private AuthenticateAnonymouseResponse authResponse = new AuthenticateAnonymouseResponse();

    public ScriptableObject Questionaire;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        yield return AutenticateAsync();
        string json = JsonUtility.ToJson(Questionaire);
        yield return SaveData(json);
    }

    IEnumerator AutenticateAsync()
    {        
        string url = identitytoolkitUri + anonymousAuthentication + apiKey;

        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"returnSecureToken\":true}");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Anonymous Authorization Erro: " + request.error);
        }
        else
        {
            Debug.Log("Anonymous Authorization OK");
            Debug.Log("Status Code: " + request.responseCode);
            authResponse = JsonUtility.FromJson<AuthenticateAnonymouseResponse>(request.downloadHandler.text);
            Debug.Log("ID" + authResponse.localId);
        }
     
    }

    IEnumerator SaveData(string json)
    {
        string url = realTimeDatabase + authResponse.localId + "/data.json?auth=" + authResponse.idToken;

        var request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log("Put Data Erro: " + request.error);
        }
        else
        {
            Debug.Log("Put Data OK");
            Debug.Log("Status Code: " + request.responseCode);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
class AuthenticateAnonymouseResponse
{
    public string kind;
    public string idToken;
    public string refreshToken;
    public int expiresIn;
    public string localId;
}
